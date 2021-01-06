using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Zs.App.Home.Web.Areas.Admin.Models.ServicesInfo;
using Zs.Bot.Data.Models;
using Zs.Common.Enums;

namespace Zs.App.Home.Web.Areas.Admin.Services
{
    /// <summary>
    /// Service that getting information about observed services (daemons)
    /// </summary>
    public class ServicesInfoService
    {
        private readonly IConfiguration _configuration;

        public ServicesInfoService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        internal List<Service> GetServicesInfoList()
        {
            var connectionStrings = _configuration.GetSection("ConnectionStrings")
                .GetChildren().Select(i => i.Value);

            var dbNames = new List<string>();
            var csBuilder = new DbConnectionStringBuilder();
            foreach (var connString in connectionStrings)
            {
                csBuilder.ConnectionString = connString;
                dbNames.Add((string)csBuilder["Database"]);
            }

            return dbNames.Distinct().Select(n => new Service { Name = n }).ToList();
        }


        private List<Log> GetLastLog(int itemsCount, LogType type)
        {
            throw new NotImplementedException();
        }

        private DbInfo GetDbInfo(string connectionString)
        {
            throw new NotImplementedException();
        }
    }
}
