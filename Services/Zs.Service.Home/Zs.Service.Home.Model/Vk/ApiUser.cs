using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Zs.Common.Extensions;
using Zs.Service.Home.Model.Db;

namespace Zs.Service.Home.Model.Vk
{
    public class ApiUser
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("is_closed")]
        public bool IsClosed { get; set; }

        [JsonPropertyName("online")]
        public int Online { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement> RawData { get; set; }


        public static explicit operator DbVkUser(ApiUser apiVkUser)
        {
            var options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            };

            return new DbVkUser()
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
}
