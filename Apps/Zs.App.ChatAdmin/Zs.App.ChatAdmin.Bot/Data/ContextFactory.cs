using System;
using System.Diagnostics;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Zs.App.ChatAdmin.Abstractions;
using Zs.Bot.Data;

namespace Zs.App.ChatAdmin.Data
{
    public class ContextFactory : IContextFactory, IDesignTimeDbContextFactory<ChatAdminContext>
    {
        private static DbContextOptions<BotContext> _botOptions;
        private static DbContextOptions<ChatAdminContext> _chatAdminOptions;

        public ChatAdminContext GetChatAdminContext()
            => new ChatAdminContext(_chatAdminOptions);

        public BotContext GetBotContext()
            => new BotContext(_botOptions);

        public ContextFactory()
        {
        }

        public ContextFactory(
            DbContextOptions<BotContext> botOptions,
            DbContextOptions<ChatAdminContext> chatAdminOptions)
        {
            _botOptions = botOptions ?? throw new ArgumentNullException(nameof(botOptions));
            _chatAdminOptions = chatAdminOptions ?? throw new ArgumentNullException(nameof(chatAdminOptions));
        }

        public ChatAdminContext CreateDbContext(string[] args)
        {
            Trace.WriteLineIf(args != null && args.Length > 0, string.Join(',', args));
            
            var solutionDir = Common.Extensions.Path.TryGetSolutionPath();
            var configuration = new ConfigurationBuilder()
                // TODO: exclude hardcoded config file name
                .AddJsonFile(Path.Combine(solutionDir, "PrivateConfiguration.json"), optional: true)
                .Build();
            var connectionString = configuration.GetConnectionString("Default");

            var optionsBuilder = new DbContextOptionsBuilder<ChatAdminContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new ChatAdminContext(optionsBuilder.Options);
        }
    }
}
