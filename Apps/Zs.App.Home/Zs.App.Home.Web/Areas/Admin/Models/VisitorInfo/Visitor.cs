using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

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
            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            _connectionJson = JsonSerializer.Serialize(new 
            {
               ClientAddress = $"{httpContext.Connection.RemoteIpAddress}:{httpContext.Connection.RemotePort}",
               httpContext.Connection.ClientCertificate
            }, jsonSerializerOptions);
            _headersJson = JsonSerializer.Serialize(httpContext.Request.Headers, jsonSerializerOptions);
            _cookiesJson = JsonSerializer.Serialize(httpContext.Request.Cookies, jsonSerializerOptions);
            _hostJson = JsonSerializer.Serialize(httpContext.Request.Host, jsonSerializerOptions);
        }

    }
}
