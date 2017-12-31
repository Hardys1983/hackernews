using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;

namespace HackerNews.HackerNews
{
    public class NewsProcessor
    {
        private readonly RestClient _itemsIdsClient = new RestClient("https://hacker-news.firebaseio.com/v0/topstories.json");
        private readonly RestClient _itemClient  = new RestClient("https://hacker-news.firebaseio.com/v0/item/");

        public IEnumerable<int> GetTopNews()
        {
            var request = new RestRequest(Method.GET);
            request.AddUrlSegment("print", "pretty");

            var response = _itemsIdsClient.Execute(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<IEnumerable<int>>(response.Content);
            }

            throw new Exception("Could not retrieve the ids");
        }

        public HackerNewsModel GetItem(int id)
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