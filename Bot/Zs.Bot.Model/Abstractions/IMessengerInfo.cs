using System;

namespace Zs.Bot.Model.Abstractions
{
    /// <summary> Messaging system information </summary>
    public interface IMessengerInfo
    {
        string Code { get; set; }
        string Name { get; set; }
        DateTime UpdateDate { get; set; }
        DateTime InsertDate { get; set; }
    }
}
