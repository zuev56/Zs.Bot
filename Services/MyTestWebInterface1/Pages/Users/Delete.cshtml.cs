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
    public class DeleteModel : PageModel
    {
        private readonly ZsBotDbContext _context;

        public DeleteModel(ZsBotDbContext context)
        {
            _context = context;
        }

        [BindProperty]
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

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ChatUser = await _context.Users.FindAsync(id);

            if (ChatUser != null)
            {
                _context.Users.Remove((DbUser)ChatUser);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
