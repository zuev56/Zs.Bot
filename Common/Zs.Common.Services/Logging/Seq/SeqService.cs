using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Zs.Common.Services.Abstractions;
using Zs.Common.Services.WebAPI;

namespace Zs.Common.Services.Logging.Seq
{
    /// <summary> Provides access to Seq's log </summary>
    public class SeqService : ISeqService
    {
        private readonly string _url;  //Example: http://localhost:5341
        private readonly string _token;

        public SeqService(string url, string token)
        {
            _url = url.TrimEnd('/');
            _token = token;
        }

        public async Task<List<string>> GetLastEvents(int take = 10, params int[] signals)
            => await GetLastEvents(DateTime.MinValue, take, signals);

        public async Task<List<string>> GetLastEvents(DateTime fromDate, int take = 10, params int[] signals)
        {
            if (signals == null)
                throw new ArgumentNullException(nameof(signals));

            if (signals.Length == 0)
                throw new ArgumentException($"{nameof(signals)} must have 1 or more elements", nameof(signals));

            var uri = CreateUri(signals, take);
            using var jsonDocument = await ApiHelper.GetAsync<JsonDocument>(uri, throwExceptionOnError: true);

            var events = new List<string>();
            GetEvents(jsonDocument).Where(e => e.Date > fromDate).ToList()
                .ForEach(e => events.Add(EventToString(e)));

            return events;
        }

        //public async Task<string> GetLastEvents(string queryOrFilter, int take = 10)
        //{
        //    throw new NotImplementedException();
        //}

        private IEnumerable<SeqEvent> GetEvents(JsonDocument jsonDocument)
        {
            foreach (var jsonElement in jsonDocument.RootElement.EnumerateArray())
            {
                var eventProperties = jsonElement.EnumerateObject().ToDictionary(p => p.Name);

                yield return new SeqEvent
                {
                    Date = eventProperties["Timestamp"].Value.GetDateTime(),
                    Properties = eventProperties["Properties"].Value.EnumerateArray().Select(o =>
                    {
                        var props = o.EnumerateObject();
                        return $"{props.First().Value.GetString()}: {props.Last().Value.GetString()}";
                    }).ToList(),
                    Messages = eventProperties["MessageTemplateTokens"].Value.EnumerateArray().SelectMany(p => p.EnumerateObject()).Select(p => p.Value.GetString()).ToList(),
                    Level = eventProperties["Level"].Value.GetString(),
                    LinkPart = eventProperties["Links"].Value.EnumerateObject().First().Value.GetString()
                };
            }
        }

        private string EventToString(SeqEvent seqEvent)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("[{0}]: ", seqEvent.Level).Append(seqEvent.Date).AppendLine();
            seqEvent.Properties.ForEach(p => sb.Append(p).AppendLine());
            seqEvent.Messages.ForEach(m => sb.Append(m).AppendLine());
            sb.Append(_url).Append('/').Append(seqEvent.LinkPart.Substring(0, seqEvent.LinkPart.IndexOf('{'))).Append("?render");

            return sb.ToString();
        }

        private string CreateUri(int[] signals, int? take = null)
        {
            var uri = signals.Length == 1
                ? $"{_url}/api/events?apikey={_token}&signal=signal-{signals[0]}"
                : $"{_url}/api/events?apikey={_token}&signal=({string.Join('~', signals.Select(s => $"signal-{s}"))})";

            return take.HasValue ? $"{uri}&count={take}" : uri;
        }

    }

}
