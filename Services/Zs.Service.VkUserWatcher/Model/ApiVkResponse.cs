using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Zs.Service.VkUserWatcher.Model
{
    public class ApiVkResponse
    {
        public List<ApiVkUser> this[int index]
            => Users;
        
        [JsonPropertyName("response")]
        public List<ApiVkUser> Users { get; set; }
    }
}
