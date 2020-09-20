using System;

namespace Zs.Service.ChatAdmin.Abstractions
{
    /// <summary> Информация о банах </summary>
    public interface IBan
    {
        int Id { get; set; }
        int UserId { get; set; }
        int ChatId { get; set; }
        int? WarningMessageId { get; set; }
        DateTime? FinishDate { get; set; }
        DateTime UpdateDate { get; set; }
        DateTime InsertDate { get; set; }
    }
}
