using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Zs.App.Home.Web.Areas.Admin.Models.VisitorInfo
{
    public class Visitor
    {
        private string _connectionJson;
        private string _headersJson;
        private string _cookiesJson;
        private string _hostJson;

        public string Json => string.Join(Environment.NewLine, _connectionJson, _headersJson);

        public Visitor(HttpContext httpContext)
        {
            var serializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore

            };

            _connectionJson = JsonConvert.SerializeObject(new 
            {
               ClientAddress = $"{httpContext.Connection.RemoteIpAddress}:{httpContext.Connection.RemotePort}",
               httpContext.Connection.ClientCertificate
            }, serializerSettings);
            _headersJson = JsonConvert.SerializeObject(httpContext.Request.Headers, serializerSettings);
            _cookiesJson = JsonConvert.SerializeObject(httpContext.Request.Cookies, serializerSettings);
            _hostJson = JsonConvert.SerializeObject(httpContext.Request.Host, serializerSettings);
        }

    }
}
