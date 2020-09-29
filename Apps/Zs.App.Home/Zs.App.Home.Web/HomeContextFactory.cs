using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Zs.App.Home.Model.Data;
using Zs.Bot.Model.Data;
using Zs.Common.Abstractions;

namespace Zs.App.Home.Web
{
    public class HomeContextFactory : IContextFactory<HomeContext>
    {
        private static DbContextOptions<HomeContext> _options;

        public HomeContextFactory()
        {
        }

        public HomeContextFactory(DbContextOptions<HomeContext> options)
        {
            _options = options;
        }

        public HomeContext GetContext()
        {
            return new HomeContext(_options);
        }

    }
}
