using System;
using Microsoft.EntityFrameworkCore;

namespace Zs.Service.VkUserWatcher.Model
{
    public partial class VkUserWatcherDbContext : DbContext
    {
        private static DbContextOptions<VkUserWatcherDbContext> _options;

        public VkUserWatcherDbContext()
           : base(_options)
        {
        }

        public VkUserWatcherDbContext(DbContextOptions<VkUserWatcherDbContext> options)
            : base(options)
        {
        }

        public static void Initialize(DbContextOptions<VkUserWatcherDbContext> options)
        {
            _options = options;
        }
    }
}
