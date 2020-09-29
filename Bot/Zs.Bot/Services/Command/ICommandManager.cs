using System;
using System.Collections.Generic;
using Zs.Bot.Model;
using Zs.Bot.Model.Abstractions;

namespace Zs.Bot.Services.Command
{
    public interface ICommandManager
    {
        event Action<CommandResult> CommandCompleted;

        bool EnqueueCommand(BotCommand command);
        List<ICommand> GetDbCommands(string userRoleCode);
    }
}