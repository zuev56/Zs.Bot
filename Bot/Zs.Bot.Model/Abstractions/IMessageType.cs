using System;

namespace Zs.Bot.Model.Abstractions
{
    /// <summary> Message Type </summary>
    public interface IMessageType
    {
        string Code { get; set; }
        string Name { get; set; }
        DateTime UpdateDate { get; set; }
        DateTime InsertDate { get; set; }
    }
}
