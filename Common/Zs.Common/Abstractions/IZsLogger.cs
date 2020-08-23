using System;

namespace Zs.Common.Abstractions
{

    public interface IZsLogger
    {
        string EmergencyLogDirrectory { get; set; }

        void LogError(Exception ex, string initiator = null);
        void LogInfo(string message, string initiator = null);
        void LogInfo<T>(string message, T data, string initiator = null);
        void LogWarning(string message = null, string initiator = null);
        void LogWarning<T>(string message, T data, string initiator = null);
    }
}
