using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;
using System.Text;
using Zs.App.Home.Data.Models.Vk;
using Zs.Bot.Data;
using Zs.Common.Extensions;

namespace Zs.App.Home.Data
{
    public partial class HomeContext : DbContext
    {
        public DbSet<ActivityLogItem> VkActivityLog { get; set; }
        public DbSet<User> VkUsers { get; set; }

        public HomeContext()
        {
        }

        public HomeContext(DbContextOptions<HomeContext> options)
           : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseSerialColumns();

            BotContext.SetDefaultValues(modelBuilder);
            BotContext.SeedData(modelBuilder);

            SetDefaultValues(modelBuilder);
        }

        private void SetDefaultValues(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActivityLogItem>().Property(b => b.InsertDate).HasDefaultValueSql("now()");

            modelBuilder.Entity<User>().Property(b => b.UpdateDate).HasDefaultValueSql("now()");
            modelBuilder.Entity<User>().Property(b => b.InsertDate).HasDefaultValueSql("now()");
        }

        public static string GetOtherSqlScripts(string dbName = null)
        {
            var resources = new[]
            {
                "Priveleges.sql",
                "StoredFunctions.sql",
                "Views.sql"
            };

            var sb = new StringBuilder();
            foreach (var resourceName in resources)
            {
                var sqlScript = Assembly.GetExecutingAssembly().ReadResource(resourceName);
                sb.Append(sqlScript + Environment.NewLine);
            }

            if (!string.IsNullOrWhiteSpace(dbName))
                sb.Replace("DefaultDbName", dbName);

            return sb.ToString();
        }
    }
}
