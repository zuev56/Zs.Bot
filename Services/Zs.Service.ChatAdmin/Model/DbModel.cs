using Microsoft.EntityFrameworkCore;

namespace Zs.Service.ChatAdmin.Model
{
    public partial class ChatAdminDbContext
    {
        private static DbContextOptions<ChatAdminDbContext> _options;

        public ChatAdminDbContext()
            : base(_options)
        {
        }

        public ChatAdminDbContext(DbContextOptions<ChatAdminDbContext> options)
           : base(options)
        {
        }

        public static void Initialize(DbContextOptions<ChatAdminDbContext> options)
        {
            _options = options;
        }
    }
}
