using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Npgsql;
using Zs.Bot.Data.Abstractions;
using Zs.Bot.Data.Models;
using Zs.Common.Abstractions;
using Zs.Common.Exceptions;
using Zs.Common.Extensions;
using Zs.Common.Helpers;

namespace Zs.Bot.Services.Commands
{
    /// <summary>
    /// Handles commands
    /// </summary>
    public class CommandManager : ICommandManager
    {
        private readonly string _connectionString;
        private readonly IZsLogger _logger;
        private readonly IRepository<Command, string> _commandsRepo;
        private readonly IRepository<UserRole, string> _userRolesRepo;
        private readonly IItemsWithRawDataRepository<User, int> _usersRepo;
        private readonly Buffer<BotCommand> _commandBuffer;

        public event EventHandler<CommandResult> CommandCompleted;

        public CommandManager(
            string connectionString,
            IRepository<Command, string> commandsRepo,
            IRepository<UserRole, string> userRolesRepo,
            IItemsWithRawDataRepository<User, int> usersRepo,
            IZsLogger logger = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException(nameof(connectionString));

            _connectionString = connectionString;
            _commandsRepo = commandsRepo ?? throw new ArgumentNullException(nameof(commandsRepo));
            _userRolesRepo = userRolesRepo ?? throw new ArgumentNullException(nameof(userRolesRepo));
            _usersRepo = usersRepo ?? throw new ArgumentNullException(nameof(usersRepo));

            _logger = logger;

            _commandBuffer = new Buffer<BotCommand>();
            _commandBuffer.OnEnqueue += CommandBuffer_OnEnqueue;
        }

        private void CommandBuffer_OnEnqueue(object sender, BotCommand item)
        {
            Task.Run(() => ProcessCommandQueueAsync());
        }

        private void EnqueueCommand(BotCommand command)
        {
            _logger?.LogInfoAsync("Command received", command, nameof(CommandManager));
            _commandBuffer.Enqueue(command);
        }

        /// <summary>
        /// Execute command in database
        /// </summary>
        /// <param name="botCommand"></param>
        /// <returns>Execution result</returns>
        internal async Task<string> RunCommand(BotCommand botCommand)
        {
            string cmdExecResult = null;
            try
            {
                var dbCommand = await _commandsRepo.FindAsync(c => c.Id == botCommand.Name);

                if (dbCommand != null)
                {
                    // (i) SQL-запросы могут быть любые, не только функции.
                    // (i) Должны содержать параметры типа object, иначе будут проблемы при форматировании строки {0}

                    var dbUser = await _usersRepo.FindAsync(u => u.Id == botCommand.FromUserId);
                    if (dbUser is null)
                        throw new ItemNotFoundException($"User with Id = {botCommand.FromUserId} not found");

                    var userHasRights = (await GetPermissionsArrayAsync(dbUser.UserRoleId))
                        .Any(p => p.ToUpperInvariant() == "ALL"
                               || string.Equals(p, dbCommand.Group, StringComparison.InvariantCultureIgnoreCase));


                    if (userHasRights)
                    {
                        // Т.о. исключаются проблемы с форматированием строки
                        var sqlCommandStr = $"{dbCommand.Script} as \"Result\"";
                        var parameters = await ProcessParametersAsync(botCommand);

                        var queryWithParams = string.Format(sqlCommandStr, parameters);



                        //Определить спец. синтаксис для дефолтных(и не только) параметров команды,
                        //который будет расшивровываться в этом блоке и обрабатываться определённым образом
                        //
                        //    ProcessSpecifiedParametres(...)
                        
                        try
                        {
                            cmdExecResult = DbHelper.GetQueryResult(_connectionString, queryWithParams) ?? "NULL";
                        }
                        catch (PostgresException pEx)
                        {
                            pEx.Data.Add("BotCommand", botCommand);
                            _logger?.LogErrorAsync(pEx, nameof(CommandManager));
                            cmdExecResult = "Command execution: request processing error!";
                        }
                        catch (Exception ex)
                        {
                            ex.Data.Add("BotCommand", botCommand);
                            _logger?.LogErrorAsync(ex, nameof(CommandManager));

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
            catch (Exception ex)
            {
                _logger?.LogErrorAsync(ex, nameof(CommandManager));
                return $"Command '{botCommand.Name}' execution failed!";
            }

            return cmdExecResult?.Trim();
        }

        /// <summary>
        /// Changes generic parameters to theirs specific values
        /// </summary>
        /// <param name="parameters">An array, that can contain generic parametres</param>
        /// <returns>Specific parameters array</returns>
        private async Task<object[]> ProcessParametersAsync(BotCommand botCommand)
        {
            var regex = new Regex(@"<([^\s>]+)\>", RegexOptions.IgnoreCase);
            var parameters = botCommand.Parametres.Cast<object>().ToArray();

            var genericParams = parameters.Cast<string>().Where(p => regex.IsMatch(p));

            var concreteParams = new Dictionary<string, string>(genericParams.Count());

            foreach (var p in genericParams)
            {
                switch (p.ToUpperInvariant())
                {
                    case "<USERROLECODE>":
                        var user = await _usersRepo.FindAsync(u => u.Id == botCommand.FromUserId);
                        concreteParams.Add(p, $"'{user?.UserRoleId}'");
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
        private async Task ProcessCommandQueueAsync()
        {
            string logCmdName = null;
            try
            {
                while (_commandBuffer.TryDequeue(out BotCommand command))
                {
                    if (command is null)
                        continue;

                    logCmdName = command.Name;
                    var result = await RunCommand(command);

                    var maxResultLength = 4000; // max for Telegram 4096
                    if (result.Length < maxResultLength)
                    {
                        CommandCompleted?.Invoke(this, new CommandResult(command.ChatIdForAnswer, result));
                    }
                    else
                    {
                        foreach (var part in result.SplitToParts(maxResultLength))
                            CommandCompleted?.Invoke(this, new CommandResult(command.ChatIdForAnswer, part));
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Add("Command", logCmdName);
                _logger?.LogErrorAsync(ex, nameof(CommandManager));
            }
        }

        private async Task<string[]> GetPermissionsArrayAsync(string userRoleId)
        {
            if (userRoleId == null)
                throw new ArgumentNullException(nameof(userRoleId));

            var role = await _userRolesRepo.FindAsync(r => r.Id == userRoleId);

            return JsonSerializer.Deserialize<string[]>(role?.Permissions);
        }

        /// <summary>
        /// Get command list for the role from database
        /// </summary>
        /// <param name="userRoleId"></param>
        /// <returns>List of commands</returns>
        private async Task<List<Command>> GetDbCommands(string userRoleId)
        {
            if (userRoleId is null)
                throw new ArgumentNullException(nameof(userRoleId));

            var role = await _userRolesRepo.FindAsync(r => r.Id == userRoleId);
            var permissionsArray = JArray.Parse(role.Permissions).ToObject<string[]>();

            var dbCommands = permissionsArray.Any(p => string.Equals(p, "All", StringComparison.InvariantCultureIgnoreCase))
                ? await _commandsRepo.FindAllAsync()
                : await _commandsRepo.FindAllAsync(c => permissionsArray.Contains(c.Group));

            return dbCommands.Cast<Command>().ToList();
        }

        private async Task<Command> GetDbCommandAsync(string commandName)
        {
            //return await ctx.Commands.Where(c => EF.Functions.Like(c.Id, commandName)).FirstOrDefaultAsync();
            return await _commandsRepo.FindAsync(c => EF.Functions.Like(c.Id, commandName));
        }

        /// <summary> Если сообщение является командой, отправит её в очередь на обработку. Иначе вернёт false </summary>
        /// <param name="message"></param>
        /// <returns>Результат выполнения команды</returns>
        public async Task<bool> TryEnqueueCommandAsync(Message message)
        {
            try
            {
                if (message is null)
                    throw new ArgumentNullException(nameof(message));

                if (BotCommand.IsCommand(message.Text))
                {
                    var botCommand = await BotCommand.ParseMessageAsync(message, GetDbCommandAsync);

                    if (botCommand != null)
                    {
                        _logger?.LogInfoAsync("Command received", botCommand, nameof(CommandManager));
                        EnqueueCommand(botCommand);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                _logger?.LogErrorAsync(e, nameof(CommandManager));
                return false;
            }
            
        }
    }
}
