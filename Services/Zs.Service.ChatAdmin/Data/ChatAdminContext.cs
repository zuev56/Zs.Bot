using System;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Zs.Bot.Model;
using Zs.Bot.Model.Data;
using Zs.Service.ChatAdmin.Model;
using Zs.Common.Extensions;

namespace Zs.Service.ChatAdmin.Data
{
    public partial class ChatAdminContext : DbContext
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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var solutionDir = Common.Helpers.Path.TryGetSolutionPath();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(solutionDir, "PrivateConfiguration.json"), optional: false)
                .Build();
            var connectionString = configuration.GetConnectionString("ChatAdmin");
        
            optionsBuilder.UseNpgsql(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");
            modelBuilder.ForNpgsqlUseSerialColumns();

            BotContext.SetDefaultValues(modelBuilder);
            BotContext.SeedData(modelBuilder);

            SetDefaultValues(modelBuilder);
            SeedData(modelBuilder);
        }

        private void SetDefaultValues(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Accounting>().Property(b => b.UpdateDate).HasDefaultValueSql("now()");
            modelBuilder.Entity<Accounting>().Property(b => b.StartDate).HasDefaultValueSql("now()");

            modelBuilder.Entity<AuxiliaryWord>().Property(b => b.InsertDate).HasDefaultValueSql("now()");

            modelBuilder.Entity<Ban>().Property(b => b.UpdateDate).HasDefaultValueSql("now()");
            modelBuilder.Entity<Ban>().Property(b => b.InsertDate).HasDefaultValueSql("now()");

            modelBuilder.Entity<Notification>().Property(b => b.UpdateDate).HasDefaultValueSql("now()");
            modelBuilder.Entity<Notification>().Property(b => b.InsertDate).HasDefaultValueSql("now()");
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Command>().HasData(new[]
            {
                new Command() { Name = "/GetUserStatistics".ToLowerInvariant(), Script = "SELECT zl.sf_cmd_get_full_statistics({0}, {1}, {2})", DefaultArgs = "15; now()::Date; now()", Description = "Получение статистики по активности участников всех чатов за определённый период", Group = "adminCmdGroup", InsertDate = DateTime.Now },
            });
        }

        public static string GetOtherSqlScripts()
        {
            var resources = new[]
            {
                "StoredFunctions.sql"
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
