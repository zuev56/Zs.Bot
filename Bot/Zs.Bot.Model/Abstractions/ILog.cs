using System;

namespace Zs.Bot.Model.Abstractions
{
    /// <summary> Log record </summary>
    public interface ILog
    {
        long Id { get; set; }
        string Type { get; set; }
        string Initiator { get; set; }
        string Message { get; set; }
        string Data { get; set; }
        DateTime InsertDate { get; set; }
    }
}
