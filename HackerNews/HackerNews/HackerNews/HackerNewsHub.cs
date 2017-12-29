using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace HackerNews.HackerNews
{
    public class HackerNewsHub: Hub
    {
        private static NewsProcessor _newsProcessor = new NewsProcessor();
        private static ConcurrentDictionary<int, string> _news = new ConcurrentDictionary<int, string>();
        
        public HackerNewsHub()
        {
            var thread = new Thread(UpdateData);
            thread.Start();
        }

        private void UpdateData(object state)
        {
            while (true)
            {
                var changed = false;
                var ids = _newsProcessor.GetTopNews().OrderBy(value => value);

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
                        _news.TryRemove(key, out string value);
                    }
                }
               
                if (changed)
                {
                    var items = _news.Select(n => n.Value);
                    RetrieveData();
                }

                Thread.Sleep(5000);
            }
        }

        public void RetrieveData(string id)
        {
            RetrieveData();
        }

        public void RetrieveData()
        {
            var items = _news.Select(n => n.Value);
            Clients.All.updateTopHackerNews(JsonConvert.SerializeObject(items));
        }
    }
}