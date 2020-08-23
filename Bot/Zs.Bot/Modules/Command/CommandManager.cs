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
using Zs.Common.Extensions;
using Zs.Common.Abstractions;

namespace Zs.Bot.Modules.Command
{
    /// <summary>
    /// Handles commands
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
                _logger.LogInfo("Command received", command, nameof(CommandManager));
                _commandBuffer.Enqueue(command);

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, nameof(CommandManager));
                return false;
            }
        }

        /// <summary>
        /// Execute command in database
        /// </summary>
        /// <param name="botCommand"></param>
        /// <returns>Execution result</returns>
        internal string RunCommand(BotCommand botCommand)
        {
            string cmdExecResult = null;
            try
            {
                using (var ctx = new ZsBotDbContext())
                {
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
                                cmdExecResult = fromSql.ToList()?[0]?.Result ?? "NULL";
                            }
                            catch (PostgresException pEx)
                            {
                                pEx.Data.Add("BotCommand", botCommand);
                                _logger.LogError(pEx, nameof(CommandManager));
                                cmdExecResult = "Command execution: request processing error!";
                            }
                            catch (Exception ex)
                            {
                                ex.Data.Add("BotCommand", botCommand);
                                _logger.LogError(ex, nameof(CommandManager));

                                cmdExecResult = ex.Message == "Column is null"
                                    ? "NULL"
                                    : "Command execution: general error!";
                            }
                        }
                        else
                        {
                            cmdExecResult = "You have no rights for this command";
                        }
                    }
                    else
                        throw new ArgumentException($"Command '{botCommand.Name}' not found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(CommandManager));
                return $"Command '{botCommand.Name}' execution failed!";
            }

            return cmdExecResult?.Trim();
        }

        /// <summary>
        /// Changes generic parameters to theirs specific values
        /// </summary>
        /// <param name="parameters">An array, that can contain generic parametres</param>
        /// <returns>Specific parameters array</returns>
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

        /// <summary> Command queue processor </summary>
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

                    var maxResultLength = 4000; // max for Telegram 4096
                    if (result.Length < maxResultLength)
                    {
                        CommandCompleted?.Invoke(new CommandResult(command.ChatIdForAnswer, result));
                    }
                    else
                    {
                        foreach (var part in result.SplitToParts(maxResultLength))
                            CommandCompleted?.Invoke(new CommandResult(command.ChatIdForAnswer, part));
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Add("Command", logCmdName);
                _logger.LogError(ex, nameof(CommandManager));
            }
        }

        /// <summary>
        /// Get command list for the role from database
        /// </summary>
        /// <param name="userRoleCode"></param>
        /// <returns>List of commands</returns>
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

    }
}
