using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Zs.Bot.Model.Data;
using Zs.Service.ChatAdmin.Abstractions;

namespace Zs.Service.ChatAdmin.Data
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
            var solutionDir = Common.Helpers.Path.TryGetSolutionPath();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(solutionDir, "PrivateConfiguration.json"), optional: true)
                .Build();
            var connectionString = configuration.GetConnectionString("ChatAdmin");

            var optionsBuilder = new DbContextOptionsBuilder<ChatAdminContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new ChatAdminContext(optionsBuilder.Options);
        }
    }
}
