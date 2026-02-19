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
            // Android Emulator -> доступ к localhost хоста
            return "http://10.0.2.2:5242/";
#else
            // Windows локальная разработка
            return "http://localhost:5242/";
#endif

#else
            // Production (VPS)
            return "http://158.160.179.55:5242/";
#endif
        }
    }
}
