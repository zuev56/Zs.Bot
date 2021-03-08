using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Zs.Bot.Data;
using Zs.Common.Abstractions;
using Zs.App.Home.Data.Models;
using System.Diagnostics;

namespace Zs.App.Home.Data
{
    public class HomeContextFactory : IContextFactory<HomeContext>, IDesignTimeDbContextFactory<HomeContext>
    {
        private static DbContextOptions<HomeContext> _options;

        public HomeContextFactory()
        {
        }

        public HomeContextFactory(DbContextOptions<HomeContext> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        // For repositories
        public HomeContext GetContext() => new HomeContext(_options);
        
        // For migrations
        public HomeContext CreateDbContext(string[] args)
        {
            // TODO: exclude hardcoded config file name
            Trace.WriteLineIf(args != null && args.Length > 0, string.Join(',', args));

            var solutionDir = Common.Extensions.Path.TryGetSolutionPath();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(solutionDir, "PrivateConfiguration.json"), optional: true)
                .Build();
            var connectionString = configuration.GetConnectionString("Default");

            var optionsBuilder = new DbContextOptionsBuilder<HomeContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new HomeContext(optionsBuilder.Options);
        }

    }
}
