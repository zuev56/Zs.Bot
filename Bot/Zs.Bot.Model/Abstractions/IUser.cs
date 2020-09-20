using System;

namespace Zs.Bot.Model.Abstractions
{
    public interface IUser
    {
        int Id { get; set; }
        string Name { get; set; }
        string FullName { get; set; }
        string UserRoleCode { get; set; }
        bool IsBot { get; set; }
        string RawData { get; set; }
        string RawDataHash { get; set; }
        string RawDataHistory { get; set; }
        DateTime UpdateDate { get; set; }
        DateTime InsertDate { get; set; }
    }
}
