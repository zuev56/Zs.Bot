using Microsoft.EntityFrameworkCore;
using Zs.Bot.Model.Db;
using Zs.Common.Abstractions;

namespace Zs.Bot.Model
{
    public class ContextFactory : IContextFactory<ZsBotDbContext>
    {
        private static DbContextOptions<ZsBotDbContext> _options;

        public ZsBotDbContext GetContext()
        {
            return new ZsBotDbContext(_options);
        }

        public ContextFactory(DbContextOptions<ZsBotDbContext> options)
        {
            _options = options;
        }
    }
}
