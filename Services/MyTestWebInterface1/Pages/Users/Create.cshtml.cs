using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zs.Bot.Model.Db;

namespace MyTestWebInterface.Pages.Users
{
    public class CreateModel : PageModel
    {
        private readonly ZsBotDbContext _context;

        public CreateModel(ZsBotDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public IUser ChatUser { get; set; }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            _context.Users.Add((DbUser)ChatUser);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
