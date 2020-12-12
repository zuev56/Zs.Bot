using System;
using System.Threading.Tasks;
using Zs.Bot.Data.Models;

namespace Zs.Bot.Services.Commands
{
    public interface ICommandManager
    {
        event EventHandler<CommandResult> CommandCompleted;
        Task<bool> TryEnqueueCommandAsync(Message text);
    }
}