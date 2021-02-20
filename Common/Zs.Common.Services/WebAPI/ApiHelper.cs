using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Zs.Common.Extensions;

namespace Zs.Common.Services.WebAPI
{
    public static class ApiHelper
    {
        private static readonly HttpClient _client = new HttpClient();

        public static async Task<TResult> GetAsync<TResult>(
            string requestUri,
            string mediaType = null,
            string userAgent = null,
            bool throwOnError = true)
        {
            try
            {
                PrepareClient(mediaType, userAgent);
                return await _client.GetAsync<TResult>(requestUri);
            }
            catch
            {
                if (throwOnError)
                    throw;

                return default(TResult);
            }
        }

        public static async Task<string> GetAsync(
            string requestUri,
            string mediaType = null,
            string userAgent = null,
            bool throwOnError = true)
        {
            try
            {
                PrepareClient(mediaType, userAgent);
                return await _client.GetStringAsync(requestUri);
            }
            catch
            {
                if (throwOnError)
                    throw;

                return null;
            }
        }


        private static void PrepareClient(
            string mediaType = null,
            string userAgent = null)
        {
            _client.DefaultRequestHeaders.Accept.Clear();

            if (mediaType != null)
            {
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
            }

            if (userAgent != null)
            {
                _client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            }
        }
    }
}
