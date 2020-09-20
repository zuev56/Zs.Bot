using System;

namespace Zs.Bot.Model.Abstractions
{
    /// <summary> Received or sent message </summary>
    public interface IMessage
    {
        int Id { get; set; }
        int? ReplyToMessageId { get; set; }
        string MessengerCode { get; set; }
        string MessageTypeCode { get; set; }
        int UserId { get; set; }
        int ChatId { get; set; }
        string Text { get; set; }
        string RawData { get; set; }
        string RawDataHash { get; set; }
        string RawDataHistory { get; set; }
        bool IsSucceed { get; set; }
        int FailsCount { get; set; }
        string FailDescription { get; set; }
        bool IsDeleted { get; set; }
        DateTime UpdateDate { get; set; }
        DateTime InsertDate { get; set; }
    }
}
