using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Zs.Bot.Model.Data;
using Zs.Common.Abstractions;

namespace Zs.Bot.Model.Factories
{
    public class BotContextFactory : IContextFactory<BotContext>, IDesignTimeDbContextFactory<BotContext>
    {
        private static DbContextOptions<BotContext> _options;

        public BotContextFactory()
        {
        }

        public BotContextFactory(DbContextOptions<BotContext> options)
        {
            _options = options;
        }

        public BotContext GetContext()
        {
            return new BotContext(_options);
        }

        // For migrations
        public BotContext CreateDbContext(string[] args)
        {
            var solutionDir = Common.Helpers.Path.TryGetSolutionPath();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(solutionDir, "PrivateConfiguration.json"), optional: true)
                .Build();
            var connectionString = configuration.GetConnectionString("Default");

            var optionsBuilder = new DbContextOptionsBuilder<BotContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new BotContext(optionsBuilder.Options);
        }
    }
}
