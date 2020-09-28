using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Zs.App.Home.Model.VkAPI
{
    public class ApiResponse
    {
        public List<ApiUser> this[int index]
            => Users;
        
        [JsonPropertyName("response")]
        public List<ApiUser> Users { get; set; }
    }
}
