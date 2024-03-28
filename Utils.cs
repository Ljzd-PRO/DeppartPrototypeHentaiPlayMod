using System;
using System.Collections.Generic;
using Mono.Web;

namespace DeppartPrototypeHentaiPlayMod
{
    public static class Utils
    {
        public static Uri BuildRequestUri(string url, Dictionary<string, string> query)
        {
            var queryCollection = HttpUtility.ParseQueryString(string.Empty);
            foreach (var kvp in query) queryCollection[kvp.Key] = kvp.Value;
            var uriBuilder = new UriBuilder(url);
            uriBuilder.Query = queryCollection.ToString();
            return uriBuilder.Uri;
        }
    }
}