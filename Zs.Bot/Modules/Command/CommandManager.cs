using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json.Linq;
using Npgsql;
using Zs.Bot.Helpers;
using Zs.Bot.Model.Db;
using Zs.Common.Exceptions;
using Zs.Common.Interfaces;

namespace Zs.Bot.Modules.Command
{
    /// <summary>
    /// Занимается обработкой команд
    /// </summary>
    public class CommandManager
    {
        private readonly IZsLogger _logger = Logger.GetInstance();

        private readonly Buffer<BotCommand> _commandBuffer;
        
        public event Action<CommandResult> CommandCompleted;

        public CommandManager()
        {
            _commandBuffer = new Buffer<BotCommand>();
            _commandBuffer.OnEnqueue += CommandBuffer_OnEnqueue;
        }


        private void CommandBuffer_OnEnqueue(object sender, BotCommand item)
        {
            Task.Run(() => ProcessCommandQueue());
        }

        public bool EnqueueCommand(BotCommand command)
        {
            try
            {
                _logger.LogInfo("Получена команда", nameof(CommandManager));
                _commandBuffer.Enqueue(command);

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, nameof(CommandManager));
                return false;
            }
        }

        /// <summary> Выполнение команды в БД. Возвращает результат </summary>
        internal string RunCommand(BotCommand botCommand)
        {
            string cmdExecResult = null;
            try
            {
                using (var ctx = new ZsBotDbContext())
                {
                    //throw new NotImplementedException("Раскомментировать текст ниже после восстановления модели данных");

                    // Команда из таблицы Command 
                    var dbCommand = ctx.Commands.FirstOrDefault(c => c.CommandName == botCommand.Name);
                    
                    if (dbCommand != null)
                    {
                        // (i) SQL-запросы могут быть любые, не только функции.
                        // (i) Должны содержать параметры типа object, иначе будут проблемы при форматировании строки {0}

                        var dbUser = ctx.Users.FirstOrDefault(u => u.UserId == botCommand.FromUserId);
                        if (dbUser is null)
                            throw new ItemNotFoundException($"User with Id = {botCommand.FromUserId} not found");

                        var userHasRights = DbUserRole.GetPermissionsArray(dbUser.UserRoleCode)
                            .Any(p => p.ToUpperInvariant() == "ALL" 
                                   || string.Equals(p, dbCommand.CommandGroup, StringComparison.InvariantCultureIgnoreCase));


                        if (userHasRights)
                        {
                            // Т.о. исключаются проблемы с форматированием строки
                            var sqlCommandStr = $"{dbCommand.CommandScript} as \"Result\"";
                            var parameters = ProcessParameters(botCommand);

                            var queryWithParams = string.Format(sqlCommandStr, parameters);



                            //Определить спец. синтаксис для дефолтных(и не только) параметров команды,
                            //который будет расшивровываться в этом блоке и обрабатываться определённым образом
                            //
                            //    ProcessSpecifiedParametres(...)

                            var fromSql = ctx.Query.FromSqlRaw($"{queryWithParams}").AsEnumerable();

                            try
                            {
                                cmdExecResult = fromSql.ToList()?[0]?.Result
                                             ?? "NULL";
                            }
                            catch (PostgresException pe)
                            {
                                pe.Data.Add("BotCommand", botCommand);
                                _logger.LogError(pe, nameof(CommandManager));
                                cmdExecResult = "Command execution: request processing error!";
                            }
                            catch (Exception e)
                            {
                                e.Data.Add("BotCommand", botCommand);
                                _logger.LogError(e, nameof(CommandManager));

                                cmdExecResult = e.Message == "Column is null"
                                    ? "NULL"
                                    : "Command execution: general error!";
                            }
                        }
                        else
                        {
                            cmdExecResult = "Sorry, you have no rights for this command";
                        }
                    }
                    else
                        throw new ArgumentException($"Command '{botCommand.Name}' not found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(CommandManager));
                return $"Command '{botCommand.Name}' running failed!";
            }

            return cmdExecResult?.Trim();
        }

        /// <summary>
        /// Здесь происходит замена обобщённых параметров на конкретные значения
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private object[] ProcessParameters(BotCommand botCommand)
        {
            var regex = new Regex(@"<([^\s>]+)\>", RegexOptions.IgnoreCase);
            var parameters = botCommand.Parametres.Cast<object>().ToArray();

            var genericParams = parameters.Cast<string>().Where(p => regex.IsMatch(p));

            var concreteParams = new Dictionary<string, string>(genericParams.Count());

            using var ctx = new ZsBotDbContext();
            foreach (var p in genericParams)
            { 
                switch (p.ToUpperInvariant())
                {
                    case "<USERROLECODE>":
                        var userRoleCode = ctx.Users.FirstOrDefault(u => u.UserId == botCommand.FromUserId)?.UserRoleCode;
                        concreteParams.Add(p, $"'{userRoleCode}'");
                        break;
                    default: 
                        concreteParams.Add(p, null);
                        break;
                }
            }

            foreach (var cp in concreteParams)
            {
                int index = parameters.IndexOf(cp.Key);
                if (index >= 0)
                    parameters[index] = cp.Value;
            }

            return parameters;
        }

        /// <summary> Обработчик очереди команд. Работает в отдельном потоке </summary>
        private void ProcessCommandQueue()
        {
            string logCmdName = null;
            try
            {
                while (_commandBuffer.TryDequeue(out BotCommand command))
                {
                    if (command is null)
                        continue;

                    logCmdName = command.Name;
                    var result = RunCommand(command);

                    CommandCompleted?.Invoke(new CommandResult(command.ChatIdForAnswer, result));
                }
            }
            catch (Exception e)
            {
                e.Data.Add("Command", logCmdName);
                _logger.LogError(e, nameof(CommandManager));
            }
        }


        /// <summary> Получение списка команд из БД </summary>
        public static List<DbCommand> GetDbCommands(string userRoleCode)
        {
            if (userRoleCode is null)
                throw new ArgumentNullException(nameof(userRoleCode));

            using var ctx = new ZsBotDbContext();
            var permissionsString = ctx.UserRoles.FirstOrDefault(r => r.UserRoleCode == userRoleCode).UserRolePermissions;
            var permissionsArray = JArray.Parse(permissionsString).ToObject<string[]>();

            var dbCommands = permissionsArray.Any(p => string.Equals(p, "All", StringComparison.InvariantCultureIgnoreCase))
                ? ctx.Commands
                : ctx.Commands.Where(c => permissionsArray.Contains(c.CommandGroup));

            return dbCommands.ToList();
        }

        /// <summary> Получение списка команд c описанием для заданной роли </summary>
        [Obsolete("В коде не должно использоваться. Выводить эту информацию через запрос к БД")]
        public static List<string> GetCommandsInfo(string userRoleCode)
        {
            if (userRoleCode is null)
                throw new ArgumentNullException(nameof(userRoleCode));

            using var ctx = new ZsBotDbContext();
            var permissionsString = ctx.UserRoles.FirstOrDefault(r => r.UserRoleCode == userRoleCode).UserRolePermissions;
            var permissionsArray = JArray.Parse(permissionsString).ToObject<string[]>();

            var dbCommands = permissionsArray.Any(p => string.Equals(p, "All", StringComparison.InvariantCultureIgnoreCase))
                ? ctx.Commands.ToList()
                : ctx.Commands.Where(c => permissionsArray.Contains(c.CommandGroup)).ToList();

            var resultList = new List<string>(dbCommands.Count);
            dbCommands.ForEach(c => resultList.Add($"{c.CommandName} - {c.CommandDesc}"));

            return resultList;
        }

    }

}
