using System.Text.Json.Serialization;

namespace Zs.App.Home.Model.VkAPI
{
    public class ApiLastSeen
    {
        [JsonPropertyName("time")]
        public int Time { get; set; }
    }
}
