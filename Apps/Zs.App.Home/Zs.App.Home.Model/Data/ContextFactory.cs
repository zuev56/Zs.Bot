using Microsoft.EntityFrameworkCore;
using Zs.App.Home.Model.Data;
using Zs.App.Home.Model;
using Microsoft.EntityFrameworkCore.Design;
using Zs.Bot.Data;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Zs.App.Home.Model.Abstractions;

namespace Zs.App.Home.Model
{
    public class ContextFactory : IContextFactory, IDesignTimeDbContextFactory<HomeContext>
    {
        private static DbContextOptions<BotContext> _botOptions;
        private static DbContextOptions<HomeContext> _homeOptions;

        public ContextFactory()
        {
        }

        public ContextFactory(
            DbContextOptions<BotContext> botOptions,
            DbContextOptions<HomeContext> homeOptions)
        {
            _botOptions = botOptions ?? throw new ArgumentNullException(nameof(botOptions));
            _homeOptions = homeOptions ?? throw new ArgumentNullException(nameof(homeOptions));
        }

        public HomeContext CreateDbContext(string[] args)
        {
            var solutionDir = Common.Helpers.Path.TryGetSolutionPath();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(solutionDir, "PrivateConfiguration.json"), optional: true)
                .Build();
            var connectionString = configuration.GetConnectionString("Default");

            var optionsBuilder = new DbContextOptionsBuilder<HomeContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new HomeContext(optionsBuilder.Options);
        }

        public HomeContext GetHomeContext()
            => new HomeContext(_homeOptions);

        public BotContext GetBotContext()
            => new BotContext(_botOptions);
    }
}
