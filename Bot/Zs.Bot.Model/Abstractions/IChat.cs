using System;

namespace Zs.Bot.Model.Abstractions
{
    /// <summary> Chat info </summary>
    public interface IChat
    {
        int Id { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        string ChatTypeCode { get; set; }
        string RawData { get; set; }
        string RawDataHash { get; set; }
        string RawDataHistory { get; set; }
        DateTime UpdateDate { get; set; }
        DateTime InsertDate { get; set; }
    }
}
