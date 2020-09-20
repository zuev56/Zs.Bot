using Microsoft.EntityFrameworkCore;

namespace Zs.Bot.Model.Db
{
    public partial class ZsBotDbContext : DbContext
    {
        public virtual DbSet<DbQuery> Query { get; set; }

        public ZsBotDbContext(DbContextOptions<ZsBotDbContext> options)
            : base(options)
        {
        }
    }
}
