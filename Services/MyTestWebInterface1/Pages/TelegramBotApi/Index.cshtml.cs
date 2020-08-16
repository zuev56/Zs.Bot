using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zs.Bot.Model.Db;

namespace MyTestWebInterface.Pages.TelegramBotApi
{
    public class IndexModel : PageModel
    {
        private readonly ZsBotDbContext _context;

        [BindProperty(SupportsGet = true)]
        public string BotToken { get; set; }

        public string JsonResult { get; set; }

        //public IList<IUser> ChatUsers { get; set; }

        //[BindProperty(SupportsGet = true)]
        //public string SearchString { get; set; }

        //public SelectList Genres { get; set; }

        //[BindProperty(SupportsGet = true)]
        //public string MovieGenre { get; set; }


        public IndexModel(ZsBotDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            var request = new HttpWebRequest;

            HttpContext.Request.Path = $"https://api.telegram.org/bot{BotToken}/getMe";
            var message = HttpContext.Request.Body;

            JsonResult = $"GetMe: {BotToken}";
            //var query = _context.Users.AsQueryable();

            //if (!string.IsNullOrEmpty(SearchString))
            //    query = query.Where(s => s.UserName.Contains(SearchString) || s.UserFullName.Contains(SearchString));

            //Genres = new SelectList(await genreQuery.Distinct().ToListAsync());
            //var queryResult = await query.Cast<IUser>().ToListAsync();
            //ChatUsers = queryResult;
        }
    }
}
