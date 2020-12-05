using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Zs.Common.Enums;
using Zs.Common.Exceptions;
using Zs.Common.Extensions;

namespace Zs.Bot.Data.Models
{
    public partial class DbUserRoleExtensions
    {
        public static async Task<string[]> GetPermissionsArray(string userRoleId, BotContext context)
        {
            if (userRoleId == null)
                throw new ArgumentNullException(nameof(userRoleId));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var role = await context.UserRoles.FirstOrDefaultAsync(r => r.Id == userRoleId);

            return JsonSerializer.Deserialize<string[]>(role?.Permissions);
        }
    }

    public static class DbLogExtensions
    {
        private static int _logMessageMaxLength = -1;

        public static bool SaveToDb(LogType type, string message, BotContext context, string initiator = null, string data = null)
        {
            try
            {
                if (_logMessageMaxLength == -1)
                {
                    var attribute = (StringLengthAttribute)typeof(Log).GetProperty(nameof(Log.Message)).GetCustomAttributes(true)
                        .FirstOrDefault(a => a is StringLengthAttribute);
                    _logMessageMaxLength = attribute?.MaximumLength ?? 100;
                }

                if (message.Length > _logMessageMaxLength)
                    message = message.Substring(0, _logMessageMaxLength - 3) + "...";

                context.Logs.Add(new Log
                {
                    Type = type.ToString(),
                    Message = message,
                    Initiator = initiator,
                    Data = data,
                    InsertDate = DateTime.Now
                });
                return context.SaveChanges() == 1;
            }
            catch
            {
                return false;
            }
        }
    }
}
