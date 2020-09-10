using Microsoft.EntityFrameworkCore;
using Zs.Common.Abstractions;
using Zs.Service.Home.Model.Db;

namespace Zs.Service.Home.Model
{
    public class ContextFactory : IContextFactory<HomeDbContext>
    {
        private static DbContextOptions<HomeDbContext> _options;

        public HomeDbContext GetContext()
        {
            return new HomeDbContext(_options);
        }

        public ContextFactory(DbContextOptions<HomeDbContext> options)
        {
            _options = options;
        }
    }
}
