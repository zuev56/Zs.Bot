using System;

namespace Zs.Bot.Model.Abstractions
{
    /// <summary> Chat members </summary>
    public interface IUserRole
    {
        string Code { get; set; }
        string Name { get; set; }
        string Permissions { get; set; }
        DateTime UpdateDate { get; set; }
        DateTime InsertDate { get; set; }
    }
}
