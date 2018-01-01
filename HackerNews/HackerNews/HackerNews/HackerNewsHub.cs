using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HackerNews.HackerNews
{
    public class HackerNewsHub: Hub
    {
        private static NewsProcessor _newsProcessor = new NewsProcessor();
        private static ConcurrentDictionary<int, HackerNewsModel> _news = new ConcurrentDictionary<int, HackerNewsModel>();
        private static ConcurrentDictionary<string, string> _clientSearches = new ConcurrentDictionary<string, string>();
        private static ConcurrentQueue<int> _idsOrder = new ConcurrentQueue<int>();

        private static object _threadMutex = new Object();
        private static Thread _thread; 

        public HackerNewsHub()
        {
            if (_thread == null)
            {
                lock (_threadMutex)
                {
                    if (_thread == null)
                    {
                        _thread = new Thread(UpdateData);
                        _thread.Start();
                    }
                }
            }
        }

        private void UpdateData(object state)
        {
            while (true)
            {
                var changed = false;
                var ids = _newsProcessor.GetTopNews();

                foreach (var id in ids)
                {
                    if (!_news.Keys.Contains(id))
                    {
                        _news.TryAdd(id, _newsProcessor.GetItem(id));
                        changed = true;
                    }
                }

                foreach (var key in _news.Keys)
                {
                    if (!ids.Contains(key))
                    {
                        _news.TryRemove(key, out HackerNewsModel value);
                    }
                }

                var positionsChanged = !_idsOrder.SequenceEqual(ids);
                if (changed || positionsChanged)
                {
                    if (positionsChanged)
                    {
                        _idsOrder = new ConcurrentQueue<int>(ids);
                    }

                    RetrieveData();
                }

                Task.Delay(1000).Wait();
            }
        }

        public void FilterBy(string filter)
        {
            _clientSearches[GetClientIdentifier(Context)] = filter;
            FilterResultForClient(_news.Values, GetClientIdentifier(Context), filter);
        }

        public static bool StringContains(string data, string needle)
        {
            return data.IndexOf(needle, System.StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        private void FilterResultForClient(IEnumerable<HackerNewsModel> items, string connectionId, string filter)
        {
            var result = items
                    .Where(item => string.IsNullOrEmpty(filter) || StringContains(item.Title, filter) || StringContains(item.By, filter))
                    .OrderByDescending(item => item.Score);

            Clients.Clients(new[] { connectionId }).updateTopHackerNews(JsonConvert.SerializeObject(result));
        }

        public void RetrieveData()
        {
            var items = _news.Values.OrderByDescending(item => item.Score);

            var withFilters = _clientSearches
                                    .Where(pair => !string.IsNullOrEmpty(pair.Value))
                                    .Select(x => x.Key).ToArray();

            Clients.AllExcept(withFilters).updateTopHackerNews(JsonConvert.SerializeObject(items));

            foreach (var searchConnection in _clientSearches.Where(pair => !string.IsNullOrEmpty(pair.Value)))
            {
                FilterResultForClient(items, searchConnection.Key, searchConnection.Value);
            }
        }

        public override Task OnConnected()
        {
            _clientSearches.TryAdd(GetClientIdentifier(Context), null);
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            _clientSearches.TryRemove(GetClientIdentifier(Context), out string RemovedSearchValue);
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            _clientSearches.TryGetValue(GetClientIdentifier(Context), out string SearchValue);
            _clientSearches.AddOrUpdate(GetClientIdentifier(Context), SearchValue, (key, oldValue) => oldValue);

            return base.OnReconnected();
        }

        private static string GetClientIdentifier(HubCallerContext context)
        {
            return context.ConnectionId;
        }
    }
}