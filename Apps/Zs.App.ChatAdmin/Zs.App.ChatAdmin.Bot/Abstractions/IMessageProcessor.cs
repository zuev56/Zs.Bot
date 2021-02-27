using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zs.Bot.Data.Models;

namespace Zs.App.ChatAdmin.Abstractions
{

    internal interface IMessageProcessor
    {
        event Action<string> LimitsDefined;
        Task ProcessGroupMessage(Message message);
        void ResetLimits();
        Task SetInternetRepairDate(DateTime? date);
    }
}
