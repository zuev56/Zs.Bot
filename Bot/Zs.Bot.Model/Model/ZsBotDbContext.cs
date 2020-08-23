using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Zs.Common.Abstractions;

namespace Zs.Bot.Model.Db
{
    public partial class ZsBotDbContext : DbContext
    {
        private static DbContextOptions<ZsBotDbContext> _options;

        public virtual DbSet<DbQuery> Query { get; set; }

        public ZsBotDbContext()
            : base(_options)
        {
        }

        public ZsBotDbContext(DbContextOptions<ZsBotDbContext> options)
            : base(options)
        {
        }

        public static void Initialize(DbContextOptions<ZsBotDbContext> options)
        {
            _options = options;
        }

        public static string GetStringQueryResult(string query)
        {
            using var ctx = new ZsBotDbContext();
            var fromSqlRaw = ctx.Query.FromSqlRaw($"{query} as Result").AsEnumerable();

            return fromSqlRaw.ToList()?[0]?.Result ?? "NULL";
        }
    }
}
