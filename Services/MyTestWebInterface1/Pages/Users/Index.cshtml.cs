using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Zs.Bot.Model.Db;

namespace MyTestWebInterface.Pages.Users
{
    public class IndexModel : PageModel
    {
        private readonly ZsBotDbContext _context;

        public IList<IUser> ChatUsers { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        public SelectList Genres { get; set; }

        [BindProperty(SupportsGet = true)]
        public string MovieGenre { get; set; }


        public IndexModel(ZsBotDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(SearchString))
                query = query.Where(s => s.UserName.Contains(SearchString) || s.UserFullName.Contains(SearchString));

            //Genres = new SelectList(await genreQuery.Distinct().ToListAsync());
            var queryResult = await query.Cast<IUser>().ToListAsync();
            ChatUsers = queryResult;
        }
    }
}
