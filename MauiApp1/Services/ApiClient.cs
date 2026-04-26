using System;
using System.Net.Http;

namespace MauiApp1.Services
{
    public class ApiClient
    {
        public HttpClient Http { get; }

        public ApiClient()
        {
            var baseUrl = GetBaseUrl();

            Http = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
        }

        private static string GetBaseUrl()
        {
#if DEBUG

#if ANDROID
            // Android Debug -> VPS API
            return "http://159.194.232.38:8080/";
#else
            // Windows Debug -> VPS API
            return "http://159.194.232.38:8080/";
#endif

#else
            // Production (VPS)
            return "http://159.194.232.38:8080/";
#endif
        }
    }
}
