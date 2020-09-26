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

namespace Zs.Bot.Model.Data
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
        public DbSet<User> Users { get; set; }
        //public DbSet<DbQuery> Query { get; set; }

        public BotContext()
        {
        }

        public BotContext(DbContextOptions<BotContext> options)
            : base(options)
        {
        }

        public BotContext(DbContextOptions options)
            : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ForNpgsqlUseSerialColumns();

            SetDefaultValues(modelBuilder);
            
            SeedData(modelBuilder);

            //modelBuilder.Entity<Bot>().ToTable("bots", "bot");
            //modelBuilder.Entity<ChatType>().ToTable("chat_types", "bot");
            //modelBuilder.Entity<Chat>().ToTable("chats", "bot");
            //modelBuilder.Entity<Command>().ToTable("commands", "bot");
            //modelBuilder.Entity<Log>().ToTable("logs");
            //modelBuilder.Entity<MessageType>().ToTable("message_types", "bot");
            //modelBuilder.Entity<Message>().ToTable("messages", "bot");
            //modelBuilder.Entity<MessengerInfo>().ToTable("messengers", "bot");
            //modelBuilder.Entity<UserRole>().ToTable("user_roles", "bot");
            //modelBuilder.Entity<User>().ToTable("users", "bot");
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
                new MessengerInfo() { Code = "TG", Name = "Telegram", InsertDate = DateTime.Now },
                new MessengerInfo() { Code = "VK", Name = "Вконтакте", InsertDate = DateTime.Now },
                new MessengerInfo() { Code = "SK", Name = "Skype", InsertDate = DateTime.Now },
                new MessengerInfo() { Code = "FB", Name = "Facebook", InsertDate = DateTime.Now },
                new MessengerInfo() { Code = "DC", Name = "Discord", InsertDate = DateTime.Now }
            });

            modelBuilder.Entity<ChatType>().HasData(new[]
            {
                new ChatType() { Code = "CHANNEL", Name = "Channel", InsertDate = DateTime.Now },
                new ChatType() { Code = "GROUP", Name = "Group", InsertDate = DateTime.Now },
                new ChatType() { Code = "PRIVATE", Name = "Private", InsertDate = DateTime.Now },
                new ChatType() { Code = "UNDEFINED", Name = "Undefined", InsertDate = DateTime.Now }
            });

            modelBuilder.Entity<Chat>().HasData(new[]
            {
                new Chat() { Id = -1, Name = "UnitTestChat", Description = "UnitTestChat", ChatTypeCode = "PRIVATE", RawData = "{ \"test\": \"test\" }", RawDataHash = "-1063294487", InsertDate = DateTime.Now },
                new Chat() { Id = 1, Name = "zuev56", ChatTypeCode = "PRIVATE", RawData = "{ \"Id\": 210281448 }", RawDataHash = "-1063294487", InsertDate = DateTime.Now }
            });

            modelBuilder.Entity<UserRole>().HasData(new[]
            {
                new UserRole() { Code = "OWNER", Name = "Owner", Permissions = "[ \"All\" ]", InsertDate = DateTime.Now },
                new UserRole() { Code = "ADMIN", Name = "Administrator", Permissions = "[ \"adminCmdGroup\", \"moderatorCmdGroup\", \"userCmdGroup\" ]", InsertDate = DateTime.Now },
                new UserRole() { Code = "MODERATOR", Name = "Moderator", Permissions = "[ \"moderatorCmdGroup\", \"userCmdGroup\" ]", InsertDate = DateTime.Now },
                new UserRole() { Code = "USER", Name = "User", Permissions = "[ \"userCmdGroup\" ]", InsertDate = DateTime.Now }
            });

            modelBuilder.Entity<User>().HasData(new[]
            {
                new User() { Id = -10, Name = "Unknown", FullName = "for exported message reading", UserRoleCode = "USER", IsBot = false, RawData = "{ \"test\": \"test\" }", RawDataHash = "-1063294487", InsertDate = DateTime.Now },
                new User() { Id = -1, Name = "UnitTestUser", FullName = "UnitTest", UserRoleCode = "USER", IsBot = false, RawData = "{ \"test\": \"test\" }", RawDataHash = "-1063294487", InsertDate = DateTime.Now },
                new User() { Id = 1, Name = "zuev56", FullName = "Сергей Зуев", UserRoleCode = "ADMIN", IsBot = false, RawData = "{ \"Id\": 210281448 }", RawDataHash = "-1063294487", InsertDate = DateTime.Now }
            });

            modelBuilder.Entity<MessageType>().HasData(new[]
            {
                new MessageType() { Code = "UKN", Name = "Unknown", InsertDate = DateTime.Now },
                new MessageType() { Code = "TXT", Name = "Text", InsertDate = DateTime.Now },
                new MessageType() { Code = "PHT", Name = "Photo", InsertDate = DateTime.Now },
                new MessageType() { Code = "AUD", Name = "Audio", InsertDate = DateTime.Now },
                new MessageType() { Code = "VID", Name = "Video", InsertDate = DateTime.Now },
                new MessageType() { Code = "VOI", Name = "Voice", InsertDate = DateTime.Now },
                new MessageType() { Code = "DOC", Name = "Document", InsertDate = DateTime.Now },
                new MessageType() { Code = "STK", Name = "Sticker", InsertDate = DateTime.Now },
                new MessageType() { Code = "LOC", Name = "Location", InsertDate = DateTime.Now },
                new MessageType() { Code = "CNT", Name = "Contact", InsertDate = DateTime.Now },
                new MessageType() { Code = "SRV", Name = "Service message", InsertDate = DateTime.Now },
                new MessageType() { Code = "OTH", Name = "Other", InsertDate = DateTime.Now }
            });

            modelBuilder.Entity<Command>().HasData(new[]
            {
                new Command() { Name = "/Test".ToLowerInvariant(), Script = "SELECT 'Test'", Description = "Тестовый запрос к боту. Возвращает ''Test''", Group = "moderatorCmdGroup", InsertDate = DateTime.Now },
                new Command() { Name = "/NullTest".ToLowerInvariant(), Script = "SELECT null", Description = "Тестовый запрос к боту. Возвращает NULL", Group = "moderatorCmdGroup", InsertDate = DateTime.Now },
                new Command() { Name = "/Help".ToLowerInvariant(), Script = "SELECT bot.sf_cmd_get_help({0})", DefaultArgs = "<UserRoleCode>", Description = "Получение справки по доступным функциям", Group = "userCmdGroup", InsertDate = DateTime.Now },
                new Command() { Name = "/SqlQuery".ToLowerInvariant(), Script = "select (with userQuery as ({0}) select json_agg(q) from userQuery q)", DefaultArgs = "select 'Pass your query as parameter in double quotes'", Description = "SQL-запрос", Group = "adminCmdGroup", InsertDate = DateTime.Now }
            });
        }

        public static string GetOtherSqlScripts()
        {
            var resources = new[]
            {
                "StoredFunctions.sql",
                "SequencesUpdate.sql"
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
