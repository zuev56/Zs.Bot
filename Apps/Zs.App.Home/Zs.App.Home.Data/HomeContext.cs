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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // optionsBuilder.UseNpgsql(connectionString) выполняется в Startup.cs
            //var solutionDir = Common.Extensions.Path.TryGetSolutionPath();
            //var configuration = new ConfigurationBuilder()
            //    .AddJsonFile(System.IO.Path.Combine(solutionDir, "PrivateConfiguration.json"), optional: false)
            //    .Build();
            //var connectionString = configuration.GetConnectionString("Default");
            //
            //optionsBuilder.UseNpgsql(connectionString);
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
