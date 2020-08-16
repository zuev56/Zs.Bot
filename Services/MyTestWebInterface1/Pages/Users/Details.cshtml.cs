using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Zs.Bot.Model.Db;

namespace MyTestWebInterface.Pages.Users
{
    public class DetailsModel : PageModel
    {
        private readonly ZsBotDbContext _context;

        public DetailsModel(ZsBotDbContext context)
        {
            _context = context;
        }

        public IUser ChatUser { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            ChatUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);

            if (ChatUser == null)
                return NotFound();
           
            return Page();
        }
    }
}
