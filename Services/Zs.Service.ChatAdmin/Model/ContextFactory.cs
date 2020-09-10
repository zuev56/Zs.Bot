using Microsoft.EntityFrameworkCore;
using Zs.Common.Abstractions;

namespace Zs.Service.ChatAdmin.Model
{
    public class ContextFactory : IContextFactory<ChatAdminDbContext>
    {
        private static DbContextOptions<ChatAdminDbContext> _options;

        public ChatAdminDbContext GetContext()
        {
            return new ChatAdminDbContext(_options);
        }

        public ContextFactory(DbContextOptions<ChatAdminDbContext> options)
        {
            _options = options;
        }
    }
}
