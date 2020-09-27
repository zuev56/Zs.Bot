using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Zs.Common.Extensions;
using Zs.Service.Home.Model;

namespace Zs.Service.Home.Model.VkAPI
{
    public class ApiUser
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("online")]
        public int Online { get; set; }
        
        [JsonPropertyName("online_mobile")]
        public int OnlineMobile { get; set; }
        
        [JsonPropertyName("online_app")]
        public int OnlineApp { get; set; }

        [JsonPropertyName("last_seen")]
        public ApiLastSeen LastSeenUnix { get; set; }
        public DateTime LastSeen => LastSeenUnix is null
                                  ? DateTime.MinValue 
                                  : LastSeenUnix.Time.FromUnixEpoch();

        [JsonExtensionData]
        public Dictionary<string, JsonElement> RawData { get; set; }


        public static explicit operator VkUser(ApiUser apiVkUser)
        {
            var options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            };

            return new VkUser()
            {
                FirstName = apiVkUser.FirstName,
                LastName = apiVkUser.LastName,
                RawData = JsonSerializer.Serialize(apiVkUser, options).NormalizeJsonString(),
                InsertDate = DateTime.Now,
                UpdateDate = DateTime.Now
            };
        }

        public override string ToString()
            => $"{Id}  {FirstName} {LastName}";
    }

    public class ApiLastSeen
    {
        [JsonPropertyName("time")]
        public int Time { get; set; }
    }
}
