using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Zs.Common.Extensions
{
    public static class HttpClientExtension
    {
        private const string _contentType = "application/json";

        /// <summary>Get specific type result</summary>
        /// <param name="httpClient">Http client</param>
        /// <param name="url">Address without parameters</param>
        /// <param name="queryParams">Query parameters</param>
        public static async Task<TResult> GetAsync<TResult>(
            this HttpClient httpClient,
            string url,
            IDictionary<string, string> queryParams = null,
            CancellationToken cancellationToken = default)
        {
            var urlWithParams = GetFullURL(url, queryParams);
            var streamTask = httpClient.GetStreamAsync(urlWithParams, cancellationToken);

            return await JsonSerializer.DeserializeAsync<TResult>(await streamTask);
        }

        /// <summary>Send post request</summary>
        /// <param name="httpClient">Http client</param>
        /// <param name="url">Address without parameters</param>
        /// <param name="postingObject">An object to post</param>
        /// <param name="queryParams">Query parameters</param>
        public static async Task<TResult> PostAsync<TResult>(
            this HttpClient httpClient,
            string url,
            object postingObject,
            IDictionary<string, string> queryParams = null,
            CancellationToken cancellationToken = default)
        {
            var serializedObject = JsonSerializer.Serialize(postingObject);
            var stringContent = new StringContent(serializedObject, Encoding.UTF8, _contentType);
            var urlWithParams = GetFullURL(url, queryParams);

            var httpResponse = await httpClient.PostAsync(urlWithParams, stringContent, cancellationToken);
            httpResponse.EnsureSuccessStatusCode();

            var streamTask = httpResponse.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync<TResult>(await streamTask);
        }

        /// <summary>Send post request</summary>
        /// <param name="httpClient">Http client</param>
        /// <param name="url">Address without parameters</param>
        /// <param name="postingObject">An object to post</param>
        /// <param name="queryParams">Query parameters</param>
        public static async Task PostAsync(
            this HttpClient httpClient,
            string url,
            object postingObject,
            IDictionary<string, string> queryParams = null)
        {
            var serializedObject = JsonSerializer.Serialize(postingObject);
            var stringContent = new StringContent(serializedObject, Encoding.UTF8, _contentType);
            var urlWithParams = GetFullURL(url, queryParams);

            var httpResponse = await httpClient.PostAsync(urlWithParams, stringContent);
            httpResponse.EnsureSuccessStatusCode();
        }

        /// <summary> Get URL with parameters </summary>
        /// <param name="url">URL without parameters</param>
        /// <param name="queryParams">Query parameters</param>
        /// <returns>Complete URL with parameters</returns>
        private static string GetFullURL(string url, IDictionary<string, string> queryParams)
        {
            if (queryParams == null || !queryParams.Any())
                return url;

            var fullUrlBuilder = new StringBuilder();
            fullUrlBuilder.AppendFormat("{0}?", url);
            foreach (var p in queryParams)
            {
                fullUrlBuilder.AppendFormat("{0}={1}&", p.Key, p.Value);
            }

            return fullUrlBuilder.ToString().TrimEnd('&');
        }
    }
}
