using System;

namespace Zs.App.ChatAdmin.Abstractions
{
    /// <summary> Информация о времени начала учёта сообщений каждого отдельного пользователя </summary>
    public interface IAccounting
    {
        int Id { get; set; }
        DateTime StartDate { get; set; }
        DateTime UpdateDate { get; set; }
    }
}
