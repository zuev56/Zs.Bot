using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zs.Service.ChatAdmin.Abstractions
{
    /// <summary> Напоминание о событиях </summary>
    public interface INotification
    {
        int Id { get; set; }
        bool IsActive { get; set; }
        string Message { get; set; }
        int? Month { get; set; }
        int Day { get; set; }
        int Hour { get; set; }
        int Minute { get; set; }
        DateTime? ExecDate { get; set; }
        DateTime UpdateDate { get; set; }
        DateTime InsertDate { get; set; }
    }
}
