using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerNews.HackerNews
{
    public class HackerNewsHub: Hub
    {
        public override Task OnConnected()
        {
            NewsProcessor.Instance.DataChanged += RetrieveData;
            NewsProcessor.Instance.ClientSearches.TryAdd(GetClientIdentifier(Context), null);

            RetrieveData(null, null);
            return base.OnConnected();
        }
        
        public override Task OnDisconnected(bool stopCalled)
        {
            NewsProcessor.Instance.ClientSearches.TryRemove(GetClientIdentifier(Context), out string RemovedSearchValue);
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            NewsProcessor.Instance.ClientSearches.TryGetValue(GetClientIdentifier(Context), out string SearchValue);
            NewsProcessor.Instance.ClientSearches.AddOrUpdate(GetClientIdentifier(Context), SearchValue, (key, oldValue) => oldValue);

            return base.OnReconnected();
        }

        private static string GetClientIdentifier(HubCallerContext context)
        {
            return context.ConnectionId;
        }

        public void FilterBy(string filter)
        {
            NewsProcessor.Instance.ClientSearches[GetClientIdentifier(Context)] = filter;
            FilterResultForClient(NewsProcessor.Instance.News.Values, GetClientIdentifier(Context), filter);
        }

        private void FilterResultForClient(IEnumerable<HackerNewsModel> items, string connectionId, string filter)
        {
            var result = items
                    .Where(item => string.IsNullOrEmpty(filter) || StringContains(item.Title, filter) || StringContains(item.By, filter))
                    .OrderByDescending(item => item.Score);

            Clients.Clients(new[] { connectionId }).updateTopHackerNews(JsonConvert.SerializeObject(result));
        }

        public void RetrieveData(object sender, EventArgs e)
        {
            var items = NewsProcessor.Instance.News.Values.OrderByDescending(item => item.Score);

            var withFilters = NewsProcessor.Instance.ClientSearches
                                    .Where(pair => !string.IsNullOrEmpty(pair.Value))
                                    .Select(x => x.Key).ToArray();

            Clients.AllExcept(withFilters).updateTopHackerNews(JsonConvert.SerializeObject(items));

            foreach (var searchConnection in NewsProcessor.Instance.ClientSearches.Where(pair => !string.IsNullOrEmpty(pair.Value)))
            {
                FilterResultForClient(items, searchConnection.Key, searchConnection.Value);
            }
        }

        private static bool StringContains(string data, string needle)
        {
            return data.IndexOf(needle, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }
    }
}