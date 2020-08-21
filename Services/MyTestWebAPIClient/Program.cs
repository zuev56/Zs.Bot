using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyTestWebAPIClient
{
    /// <summary>
    /// https://docs.microsoft.com/ru-ru/dotnet/csharp/tutorials/console-webapiclient
    /// </summary>
    class Program
    {
        private static readonly HttpClient _client = new HttpClient();

        static async Task Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("--- Start ---");
            Console.WriteLine();

            var repositories = await ProcessRepositories();
            repositories.ForEach(r => 
            Console.WriteLine($"{r.Name}: {r.Description}\n" +
                              $"{(r.GitHubHomeUrl != null ? $"{r.GitHubHomeUrl}\n" : "")}" +
                              $"{(r.Homepage != null ? $"{r.Homepage}\n" : "")}" +
                              $"{r.Watchers}\n" +
                              $"{r.LastPush}"));

            Console.WriteLine();
            Console.WriteLine("--- Finish ---");
            Console.WriteLine();
        }

        private static async Task<List<Repository>> ProcessRepositories()
        {
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));

            _client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            // v.1 Get string
            //var stringTask = _client.GetStringAsync("https://api.github.com/orgs/dotnet/repos");
            //var message = await stringTask;
            //Console.Write(message);

            // v.2 Get objects
            var streamTask = _client.GetStreamAsync("https://api.github.com/orgs/dotnet/repos");
            var repositories = await JsonSerializer.DeserializeAsync<List<Repository>>(await streamTask);
            return repositories;
        }
    }
}
