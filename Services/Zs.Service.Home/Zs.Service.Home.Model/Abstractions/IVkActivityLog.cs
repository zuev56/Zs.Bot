using System;

namespace Zs.Service.Home.Model.Abstractions
{
    /// <summary> Vk users activity log </summary>
    public interface IVkActivityLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool? IsOnline { get; set; }
        public DateTime InsertDate { get; set; }
        public int? OnlineApp { get; set; }
        public bool IsOnlineMobile { get; set; }
        public int LastSeen { get; set; }
    }
}
