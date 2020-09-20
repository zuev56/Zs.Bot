using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Zs.Bot.Model;
using Zs.Service.ChatAdmin.Model;

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
            var connectionString = configuration.GetConnectionString("ChatAdminTestCF");
        
            optionsBuilder.UseNpgsql(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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

        }
    }
}
