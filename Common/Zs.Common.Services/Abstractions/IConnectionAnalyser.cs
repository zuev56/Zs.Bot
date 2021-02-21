using System;
using System.Net;
using Zs.Common.Enums;

namespace Zs.Common.Services.Abstractions
{
    /// <summary>
    /// Analyzes internet connection
    /// </summary>
    public interface IConnectionAnalyser
    {
        /// <summary> Current connection status </summary>
        ConnectionStatus CurrentStatus { get; }

        /// <summary> The date when internet connection was repaired </summary>
        DateTime? InternetRepairDate { get; }

        /// <summary> 
        /// <see cref="System.Net.WebProxy"/> instance. 
        /// Use <see cref="InitializeProxy"/> method to initialize 
        /// </summary>
        WebProxy WebProxy { get; }

        /// <summary> Occurs when connection satus changes </summary>
        event Action<ConnectionStatus> ConnectionStatusChanged;

        /// <summary>
        /// Initialize <see cref="WebProxy" instance/>
        /// </summary>
        /// <param name="socket">IP address and port. Example: 123.45.67.89:6543</param>
        /// <param name="userName">The user name associated with the credentials</param>
        /// <param name="password">The password for the user name associated with the credentials</param>
        void InitializeProxy(string socket, string userName = null, string password = null);

        /// <summary>
        /// Starts the <see cref="IConnectionAnalyser"/>
        /// </summary>
        /// <param name="dueTime">The amount of time in milliseconds to delay before the start of the connection analysis</param>
        /// <param name="period">The time interval between analyse of the connection</param>
        void Start(uint dueTime, uint period);

        /// <summary>
        /// Stops the <see cref="IConnectionAnalyser"/>
        /// </summary>
        void Stop();
    }
}
