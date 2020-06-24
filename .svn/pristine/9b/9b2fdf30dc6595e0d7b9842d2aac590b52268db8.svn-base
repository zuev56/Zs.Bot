using System;

namespace Zs.Common.Interfaces
{

    public interface IZsLogger
    {
        string EmergencyLogDirrectory { get; set; }

        void LogError(Exception ex, string logGroup = null);
        void LogInfo(string message, string logGroup = null);
        void LogInfo<T>(string message, T logData, string logGroup = null);
        void LogWarning(string message = null, string logGroup = null);
    }
}
