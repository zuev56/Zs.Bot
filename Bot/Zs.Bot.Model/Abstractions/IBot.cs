using System;

namespace Zs.Bot.Model.Abstractions
{
    /// <summary> Bot info </summary>
    public interface IBot
    {
        int Id { get; set; }
        string MessengerCode { get; set; }
        string Name { get; set; }
        string Token { get; set; }
        string Description { get; set; }
        DateTime UpdateDate { get; set; }
        DateTime InsertDate { get; set; }
    }
}
