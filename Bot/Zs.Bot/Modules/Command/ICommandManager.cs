using System;
using System.Collections.Generic;
using Zs.Bot.Model.Db;

namespace Zs.Bot.Modules.Command
{
    public interface ICommandManager
    {
        event Action<CommandResult> CommandCompleted;

        bool EnqueueCommand(BotCommand command);
        List<DbCommand> GetDbCommands(string userRoleCode);
    }
}