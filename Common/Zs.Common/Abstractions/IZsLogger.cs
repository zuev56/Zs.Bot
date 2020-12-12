using System;
using System.Threading.Tasks;

namespace Zs.Common.Abstractions
{

    public interface IZsLogger
    {
        string EmergencyLogDirrectory { get; set; }

        Task LogErrorAsync(Exception ex, string initiator = null);
        Task LogInfoAsync(string message, string initiator = null);
        Task LogInfoAsync<T>(string message, T data, string initiator = null);
        Task LogWarningAsync(string message = null, string initiator = null);
        Task LogWarningAsync<T>(string message, T data, string initiator = null);
    }
}
