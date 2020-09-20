using System;

namespace Zs.Bot.Model.Abstractions
{
    /// <summary> Chat type (group, private, etc.) </summary>
    public interface IChatType
    {
        string Code { get; set; }
        string Name { get; set; }
        DateTime UpdateDate { get; set; }
        DateTime InsertDate { get; set; }
    }
}
