using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Zs.Service.Home.Model.Vk
{
    public class ApiResponse
    {
        public List<ApiUser> this[int index]
            => Users;
        
        [JsonPropertyName("response")]
        public List<ApiUser> Users { get; set; }
    }
}
