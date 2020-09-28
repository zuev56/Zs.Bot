using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Zs.Bot.Model.Data;
using Zs.Common.Extensions;

namespace Zs.App.Home.Model.Data
{
    public partial class HomeContext : DbContext
    {
        public DbSet<VkActivityLog> VkActivityLog { get; set; }
        public DbSet<VkUser> VkUsers { get; set; }

        public HomeContext()
        {
        }

        public HomeContext(DbContextOptions<HomeContext> options)
           : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var solutionDir = Common.Helpers.Path.TryGetSolutionPath();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(solutionDir, "PrivateConfiguration.json"), optional: false)
                .Build();
            var connectionString = configuration.GetConnectionString("Default");

            optionsBuilder.UseNpgsql(connectionString);
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
            modelBuilder.Entity<VkActivityLog>().Property(b => b.InsertDate).HasDefaultValueSql("now()");

            modelBuilder.Entity<VkUser>().Property(b => b.UpdateDate).HasDefaultValueSql("now()");
            modelBuilder.Entity<VkUser>().Property(b => b.InsertDate).HasDefaultValueSql("now()");
        }

        public static string GetOtherSqlScripts()
        {
            var resources = new[]
            {
                "StoredFunctions.sql",
                "Views.sql"
            };

            var sb = new StringBuilder();
            foreach (var resourceName in resources)
            {
                var sqlScript = Assembly.GetExecutingAssembly().ReadResource(resourceName);
                sb.Append(sqlScript + Environment.NewLine);
            }

            return sb.ToString();
        }
    }
}
