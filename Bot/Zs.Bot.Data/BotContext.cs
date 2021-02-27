using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;
using System.Text;
using Zs.Bot.Data.Models;
using Zs.Common.Extensions;

namespace Zs.Bot.Data
{
    public class BotContext : DbContext
    {
        public DbSet<ChatType> ChatTypes { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Command> Commands { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<MessageType> MessageTypes { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessengerInfo> Messengers { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        /*internal*/ public DbSet<User> Users { get; set; }

        public BotContext()
        {
        }

        public BotContext(DbContextOptions<BotContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseSerialColumns();

            SetDefaultValues(modelBuilder);
            SeedData(modelBuilder);
        }

        public static void SetDefaultValues(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MessengerInfo>().Property(b => b.UpdateDate).HasDefaultValueSql("now()");
            modelBuilder.Entity<MessengerInfo>().Property(b => b.InsertDate).HasDefaultValueSql("now()");

            modelBuilder.Entity<ChatType>().Property(b => b.UpdateDate).HasDefaultValueSql("now()");
            modelBuilder.Entity<ChatType>().Property(b => b.InsertDate).HasDefaultValueSql("now()");

            modelBuilder.Entity<Chat>().Property(b => b.UpdateDate).HasDefaultValueSql("now()");
            modelBuilder.Entity<Chat>().Property(b => b.InsertDate).HasDefaultValueSql("now()");

            modelBuilder.Entity<UserRole>().Property(b => b.UpdateDate).HasDefaultValueSql("now()");
            modelBuilder.Entity<UserRole>().Property(b => b.InsertDate).HasDefaultValueSql("now()");

            modelBuilder.Entity<User>().Property(b => b.UpdateDate).HasDefaultValueSql("now()");
            modelBuilder.Entity<User>().Property(b => b.InsertDate).HasDefaultValueSql("now()");
            
            modelBuilder.Entity<MessageType>().Property(b => b.UpdateDate).HasDefaultValueSql("now()");
            modelBuilder.Entity<MessageType>().Property(b => b.InsertDate).HasDefaultValueSql("now()");

            modelBuilder.Entity<Message>().Property(b => b.UpdateDate).HasDefaultValueSql("now()");
            modelBuilder.Entity<Message>().Property(b => b.InsertDate).HasDefaultValueSql("now()");

            modelBuilder.Entity<Log>().Property(b => b.InsertDate).HasDefaultValueSql("now()");

            modelBuilder.Entity<Command>().Property(b => b.UpdateDate).HasDefaultValueSql("now()");
            modelBuilder.Entity<Command>().Property(b => b.InsertDate).HasDefaultValueSql("now()");
        }

        public static void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MessengerInfo>().HasData(new[]
            {
                new MessengerInfo() { Id = "TG", Name = "Telegram", InsertDate = DateTime.Now },
                new MessengerInfo() { Id = "VK", Name = "Вконтакте", InsertDate = DateTime.Now },
                new MessengerInfo() { Id = "SK", Name = "Skype", InsertDate = DateTime.Now },
                new MessengerInfo() { Id = "FB", Name = "Facebook", InsertDate = DateTime.Now },
                new MessengerInfo() { Id = "DC", Name = "Discord", InsertDate = DateTime.Now }
            });

            modelBuilder.Entity<ChatType>().HasData(new[]
            {
                new ChatType() { Id = "CHANNEL", Name = "Channel", InsertDate = DateTime.Now },
                new ChatType() { Id = "GROUP", Name = "Group", InsertDate = DateTime.Now },
                new ChatType() { Id = "PRIVATE", Name = "Private", InsertDate = DateTime.Now },
                new ChatType() { Id = "UNDEFINED", Name = "Undefined", InsertDate = DateTime.Now }
            });

            modelBuilder.Entity<Chat>().HasData(new[]
            {
                new Chat() { Id = -1, Name = "UnitTestChat", Description = "UnitTestChat", ChatTypeId = "PRIVATE", RawData = "{ \"test\": \"test\" }", RawDataHash = "-1063294487", InsertDate = DateTime.Now },
                new Chat() { Id = 1, Name = "zuev56", ChatTypeId = "PRIVATE", RawData = "{ \"Id\": 210281448 }", RawDataHash = "-1063294487", InsertDate = DateTime.Now }
            });

            modelBuilder.Entity<UserRole>().HasData(new[]
            {
                new UserRole() { Id = "OWNER", Name = "Owner", Permissions = "[ \"All\" ]", InsertDate = DateTime.Now },
                new UserRole() { Id = "ADMIN", Name = "Administrator", Permissions = "[ \"adminCmdGroup\", \"moderatorCmdGroup\", \"userCmdGroup\" ]", InsertDate = DateTime.Now },
                new UserRole() { Id = "MODERATOR", Name = "Moderator", Permissions = "[ \"moderatorCmdGroup\", \"userCmdGroup\" ]", InsertDate = DateTime.Now },
                new UserRole() { Id = "USER", Name = "User", Permissions = "[ \"userCmdGroup\" ]", InsertDate = DateTime.Now }
            });

            modelBuilder.Entity<User>().HasData(new[]
            {
                new User() { Id = -10, Name = "Unknown", FullName = "for exported message reading", UserRoleId = "USER", IsBot = false, RawData = "{ \"test\": \"test\" }", RawDataHash = "-1063294487", InsertDate = DateTime.Now },
                new User() { Id = -1, Name = "UnitTestUser", FullName = "UnitTest", UserRoleId = "USER", IsBot = false, RawData = "{ \"test\": \"test\" }", RawDataHash = "-1063294487", InsertDate = DateTime.Now },
                new User() { Id = 1, Name = "zuev56", FullName = "Сергей Зуев", UserRoleId = "ADMIN", IsBot = false, RawData = "{ \"Id\": 210281448 }", RawDataHash = "-1063294487", InsertDate = DateTime.Now }
            });

            modelBuilder.Entity<MessageType>().HasData(new[]
            {
                new MessageType() { Id = "UKN", Name = "Unknown", InsertDate = DateTime.Now },
                new MessageType() { Id = "TXT", Name = "Text", InsertDate = DateTime.Now },
                new MessageType() { Id = "PHT", Name = "Photo", InsertDate = DateTime.Now },
                new MessageType() { Id = "AUD", Name = "Audio", InsertDate = DateTime.Now },
                new MessageType() { Id = "VID", Name = "Video", InsertDate = DateTime.Now },
                new MessageType() { Id = "VOI", Name = "Voice", InsertDate = DateTime.Now },
                new MessageType() { Id = "DOC", Name = "Document", InsertDate = DateTime.Now },
                new MessageType() { Id = "STK", Name = "Sticker", InsertDate = DateTime.Now },
                new MessageType() { Id = "LOC", Name = "Location", InsertDate = DateTime.Now },
                new MessageType() { Id = "CNT", Name = "Contact", InsertDate = DateTime.Now },
                new MessageType() { Id = "SRV", Name = "Service message", InsertDate = DateTime.Now },
                new MessageType() { Id = "OTH", Name = "Other", InsertDate = DateTime.Now }
            });

            modelBuilder.Entity<Command>().HasData(new[]
            {
                new Command() { Id = "/Test".ToLowerInvariant(), Script = "SELECT 'Test'", Description = "Тестовый запрос к боту. Возвращает ''Test''", Group = "moderatorCmdGroup", InsertDate = DateTime.Now },
                new Command() { Id = "/NullTest".ToLowerInvariant(), Script = "SELECT null", Description = "Тестовый запрос к боту. Возвращает NULL", Group = "moderatorCmdGroup", InsertDate = DateTime.Now },
                new Command() { Id = "/Help".ToLowerInvariant(), Script = "SELECT bot.sf_cmd_get_help({0})", DefaultArgs = "<UserRoleId>", Description = "Получение справки по доступным функциям", Group = "userCmdGroup", InsertDate = DateTime.Now },
                new Command() { Id = "/SqlQuery".ToLowerInvariant(), Script = "select (with userQuery as ({0}) select json_agg(q) from userQuery q)", DefaultArgs = "select 'Pass your query as parameter in double quotes'", Description = "SQL-запрос", Group = "adminCmdGroup", InsertDate = DateTime.Now }
            });
        }

        public static string GetOtherSqlScripts(string dbName = null)
        {
            var resources = new[]
            {
                "Priveleges.sql",
                "StoredFunctions.sql",
                "SequencesUpdate.sql"
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
