using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;
using System.Text;
using Zs.App.ChatAdmin.Model;
using Zs.Bot.Data;
using Zs.Bot.Data.Models;
using Zs.Common.Extensions;

namespace Zs.App.ChatAdmin.Data
{
    public class ChatAdminContext : DbContext
    {
        public DbSet<Accounting> Accountings { get; set; }
        public DbSet<AuxiliaryWord> AuxiliaryWords { get; set; }
        public DbSet<Ban> Bans { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public ChatAdminContext()
        {
        }

        public ChatAdminContext(DbContextOptions<ChatAdminContext> options)
           : base(options)
        {
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseSerialColumns();

            BotContext.SetDefaultValues(modelBuilder);
            BotContext.SeedData(modelBuilder);

            SetDefaultValues(modelBuilder);
            SeedData(modelBuilder);
        }

        protected void SetDefaultValues(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Accounting>().Property(b => b.UpdateDate).HasDefaultValueSql("now()");
            modelBuilder.Entity<Accounting>().Property(b => b.StartDate).HasDefaultValueSql("now()");

            modelBuilder.Entity<AuxiliaryWord>().Property(b => b.InsertDate).HasDefaultValueSql("now()");

            modelBuilder.Entity<Ban>().Property(b => b.UpdateDate).HasDefaultValueSql("now()");
            modelBuilder.Entity<Ban>().Property(b => b.InsertDate).HasDefaultValueSql("now()");

            modelBuilder.Entity<Notification>().Property(b => b.UpdateDate).HasDefaultValueSql("now()");
            modelBuilder.Entity<Notification>().Property(b => b.InsertDate).HasDefaultValueSql("now()");
        }

        protected void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Command>().HasData(new[]
            {
                new Command() { Id = "/GetUserStatistics".ToLowerInvariant(), Script = "SELECT zl.sf_cmd_get_full_statistics({0}, {1}, {2})", DefaultArgs = "15; now()::Date; now()", Description = "Получение статистики по активности участников всех чатов за определённый период", Group = "adminCmdGroup", InsertDate = DateTime.Now },
            });
        }

        public static string GetOtherSqlScripts(string dbName = null)
        {
            var resources = new[]
            {
                "Priveleges.sql",
                "StoredFunctions.sql"
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
