namespace Zs.App.Home.Web.Areas.App.Models.Vk
{

    public class UserVM
    {
        public int Id { get; init; }
        public string UserName { get; init; }
        public int ActivitySec { get; internal set; } = -1;
        //public string Url { get; set; }
    }
}
