using System;

namespace Zs.App.Home.Model.Abstractions
{
    /// <summary> Vk user </summary>
    public interface IVkUser
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string RawData { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime InsertDate { get; set; }
    }
}
