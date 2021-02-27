using System;
using System.Diagnostics;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
//using Zs.App.ChatAdmin.Abstractions;
using Zs.Bot.Data;
using Zs.Common.Abstractions;

namespace Zs.App.ChatAdmin.Data
{
    public class ContextFactory : IContextFactory<ChatAdminContext>, IDesignTimeDbContextFactory<ChatAdminContext>
    {
        private static DbContextOptions<ChatAdminContext> _options;

        public ContextFactory()
        {
        }

        public ContextFactory(DbContextOptions<ChatAdminContext> chatAdminOptions)
        {
            _options = chatAdminOptions ?? throw new ArgumentNullException(nameof(chatAdminOptions));
        }

        public ChatAdminContext GetContext() => new ChatAdminContext(_options);

        public ChatAdminContext CreateDbContext(string[] args)
        {
            // TODO: exclude hardcoded config file name
            Trace.WriteLineIf(args != null && args.Length > 0, string.Join(',', args));
            
            var solutionDir = Common.Extensions.Path.TryGetSolutionPath();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(solutionDir, "PrivateConfiguration.json"), optional: true)
                .Build();
            var connectionString = configuration.GetConnectionString("Default");

            var optionsBuilder = new DbContextOptionsBuilder<ChatAdminContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new ChatAdminContext(optionsBuilder.Options);
        }
    }
}
