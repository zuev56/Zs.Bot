using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Zs.Common.Interfaces;

namespace Zs.Bot.Model.Db
{
    public partial class ZsBotDbContext : DbContext
    {
        private static DbContextOptions<ZsBotDbContext> _options;
        private static IZsLogger _logger;

        public virtual DbSet<DbQuery> Query { get; set; }


        public ZsBotDbContext()
            : base(_options)
        {
        }

        public static void Initialize(DbContextOptions<ZsBotDbContext> options)
        {
            if (_options != null)
                throw new InvalidOperationException("Reinitialize is not allowed");

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
