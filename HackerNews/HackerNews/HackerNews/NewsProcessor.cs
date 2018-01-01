using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HackerNews.HackerNews
{
    public class NewsProcessor
    {
        public ConcurrentDictionary<int, HackerNewsModel> News { get; } = new ConcurrentDictionary<int, HackerNewsModel>();
        public ConcurrentDictionary<string, string> ClientSearches { get; } = new ConcurrentDictionary<string, string>();
        public ConcurrentQueue<int> IdsOrder { get; private set; } = new ConcurrentQueue<int>();
        public event EventHandler DataChanged;

        public static NewsProcessor Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_instanceMutex)
                    {
                        if (_instance == null)
                        {
                            _instance = new NewsProcessor();
                        }
                    }
                }

                return _instance;
            }
        }

        private readonly RestClient _itemsIdsClient = new RestClient(ConfigurationManager.AppSettings["TopNewsUrl"]);
        private readonly RestClient _itemClient  = new RestClient(ConfigurationManager.AppSettings["GetNewsItemUrl"]);
        private Thread _thread;

        private static object _instanceMutex = new Object();
        private static NewsProcessor _instance;

        private NewsProcessor()
        {
            _thread = new Thread(UpdateData);
            _thread.Start();
        }

        private void UpdateData(object state)
        {
            while (true)
            {
                var changed = false;
                var ids = GetTopNews();

                if (!ids.Any())
                {
                    continue;
                }

                foreach (var id in ids)
                {
                    if (!News.Keys.Contains(id))
                    {
                        try
                        {
                            News.TryAdd(id, GetItem(id));
                            changed = true;
                        }
                        catch
                        {
                            //TODO: Log Exception while retrieving item
                        }
                    }
                }

                foreach (var key in News.Keys)
                {
                    if (!ids.Contains(key))
                    {
                        News.TryRemove(key, out HackerNewsModel value);
                    }
                }

                var positionsChanged = !IdsOrder.SequenceEqual(ids);
                if (changed || positionsChanged)
                {
                    if (positionsChanged)
                    {
                        IdsOrder = new ConcurrentQueue<int>(ids);
                    }

                    DataChanged?.Invoke(null, null);
                }

                Task.Delay(1000).Wait();
            }
        }

        private IEnumerable<int> GetTopNews()
        {
            var request = new RestRequest(Method.GET);
            request.AddUrlSegment("print", "pretty");

            var response = _itemsIdsClient.Execute(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<IEnumerable<int>>(response.Content);
            }

            //Log issue
            return Enumerable.Empty<int>();
        }

        private HackerNewsModel GetItem(int id)
        {
            var request = new RestRequest($"{id}.json", Method.GET);
            request.AddUrlSegment("print", "pretty");

            var response = _itemClient.Execute(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return HackerNewsModel.FromJson(response.Content);
            }

            throw new Exception($"Could not retrieve the item #{id}");
        }
    }
}