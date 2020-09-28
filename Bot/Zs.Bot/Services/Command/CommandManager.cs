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
using Zs.Bot.Model;
using Zs.Bot.Model.Abstractions;
using Zs.Bot.Model.Data;
using Zs.Common.Abstractions;
using Zs.Common.Exceptions;
using Zs.Common.Extensions;
using Zs.Common.Helpers;

namespace Zs.Bot.Services.Command
{
    /// <summary>
    /// Handles commands
    /// </summary>
    public class CommandManager : ICommandManager
    {
        private readonly IZsLogger _logger;
        private readonly IContextFactory<BotContext> _contextFactory;

        private readonly Buffer<BotCommand> _commandBuffer;

        public event Action<CommandResult> CommandCompleted;

        public CommandManager(
            IContextFactory<BotContext> contextFactory,
            IZsLogger logger)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _logger = logger;

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
                using (var ctx = _contextFactory.GetContext())
                {
                    var dbCommand = ctx.Commands.FirstOrDefault(c => c.Name == botCommand.Name);

                    if (dbCommand != null)
                    {
                        // (i) SQL-запросы могут быть любые, не только функции.
                        // (i) Должны содержать параметры типа object, иначе будут проблемы при форматировании строки {0}

                        var dbUser = ctx.Users.FirstOrDefault(u => u.Id == botCommand.FromUserId);
                        if (dbUser is null)
                            throw new ItemNotFoundException($"User with Id = {botCommand.FromUserId} not found");

                        var userHasRights = DbUserRoleExtensions.GetPermissionsArray(dbUser.UserRoleCode, _contextFactory.GetContext())
                            .Any(p => p.ToUpperInvariant() == "ALL"
                                   || string.Equals(p, dbCommand.Group, StringComparison.InvariantCultureIgnoreCase));


                        if (userHasRights)
                        {
                            // Т.о. исключаются проблемы с форматированием строки
                            var sqlCommandStr = $"{dbCommand.Script} as \"Result\"";
                            var parameters = ProcessParameters(botCommand);

                            var queryWithParams = string.Format(sqlCommandStr, parameters);



                            //Определить спец. синтаксис для дефолтных(и не только) параметров команды,
                            //который будет расшивровываться в этом блоке и обрабатываться определённым образом
                            //
                            //    ProcessSpecifiedParametres(...)
                            
                            try
                            {
                                var connectionString = ctx.Database.GetDbConnection().ConnectionString;
                                cmdExecResult = DbHelper.GetQueryResult(connectionString, queryWithParams) ?? "NULL";
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

            using var ctx = _contextFactory.GetContext();
            foreach (var p in genericParams)
            {
                switch (p.ToUpperInvariant())
                {
                    case "<USERROLECODE>":
                        var userRoleCode = ctx.Users.FirstOrDefault(u => u.Id == botCommand.FromUserId)?.UserRoleCode;
                        concreteParams.Add(p, $"'{userRoleCode}'");
                        break;
                    default:
                        concreteParams.Add(p, null);
                        break;
                }
            }

            foreach (var cp in concreteParams)
            {
                int index = Array.IndexOf(parameters, cp.Key);
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
        public List<ICommand> GetDbCommands(string userRoleCode)
        {
            if (userRoleCode is null)
                throw new ArgumentNullException(nameof(userRoleCode));

            using var ctx = _contextFactory.GetContext();
            var permissionsString = ctx.UserRoles.FirstOrDefault(r => r.Code == userRoleCode).Permissions;
            var permissionsArray = JArray.Parse(permissionsString).ToObject<string[]>();

            var dbCommands = permissionsArray.Any(p => string.Equals(p, "All", StringComparison.InvariantCultureIgnoreCase))
                ? ctx.Commands
                : ctx.Commands.Where(c => permissionsArray.Contains(c.Group));

            return dbCommands.Cast<ICommand>().ToList();
        }

        internal ICommand GetDbCommand(string commandName)
        {
            using var ctx = _contextFactory.GetContext();
            var test = ctx.Commands.Where(c => EF.Functions.Like(c.Name, commandName));
            return ctx.Commands.Where(c => EF.Functions.Like(c.Name, commandName)).FirstOrDefault();
        }

    }
}
