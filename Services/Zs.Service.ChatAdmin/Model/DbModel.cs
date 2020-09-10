using Microsoft.EntityFrameworkCore;

namespace Zs.Service.ChatAdmin.Model
{
    public partial class ChatAdminDbContext
    {
        public ChatAdminDbContext(DbContextOptions<ChatAdminDbContext> options)
           : base(options)
        {
        }
    }
}
