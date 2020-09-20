using System;

namespace Zs.Bot.Model.Abstractions
{
    /// <summary> Command </summary>
    public interface ICommand
    {
        string Name { get; set; }
        string Script { get; set; }
        string DefaultArgs { get; set; }
        string Description { get; set; }
        string Group { get; set; }
        DateTime UpdateDate { get; set; }
        DateTime InsertDate { get; set; }
    }
}
