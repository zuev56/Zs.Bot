using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Zs.App.Home.Web.Areas.Admin.Models.ServerInfo
{
    public class Server
    {
        public Environment EnvironmentInfo { get; } = new Environment();
        public Memory Memory { get; }
        public List<Drive> Drives { get; } = new List<Drive>();
        public List<Process> Processes { get; } = new List<Process>();

        public Server()
        {
            //Name = Environment.MachineName;
            //IPs = Dns.GetHostAddresses(Dns.GetHostName()).Select(ip => ip.ToString()).ToList();

            foreach (var drive in System.IO.DriveInfo.GetDrives())
            {
                Drives.Add(new Drive
                {
                    Name = drive.Name,
                    Type = drive.DriveType,
                    FileSystem = drive.IsReady ? drive.DriveFormat : "Unknown",
                    TotalSize = drive.IsReady ? drive.TotalSize / (1024 * 1024) : null,
                    FreeSpace = drive.IsReady ? drive.TotalFreeSpace / (1024 * 1024) : null
                });
            }

            var allProcesses = System.Diagnostics.Process.GetProcesses()
                .Select(p => new
                {
                    Name = p.ProcessName,
                    IsResponding = p.Responding,
                    ThreadsNumber = p.Threads.Count
                });

            var grouppedProcesses = allProcesses.GroupBy(g => g.Name)
                .Select(g => new
                { 
                    Name = g.Key,
                    IsResponding = !allProcesses.Where(p => p.Name == g.Key).Any(p => !p.IsResponding),
                    ThreadsNumber = allProcesses.Where(p => p.Name == g.Key).Sum(p => p.ThreadsNumber),
                    Count = g.Count()
                })
                .OrderBy(p => p.Name);

            foreach (var process in grouppedProcesses)
            {
                Processes.Add(new Process
                {
                    Name = process.Name,
                    DuplicatesInName = process.Count,
                    IsResponding = process.IsResponding,
                    ThreadsNumber = process.ThreadsNumber
                });
            }
        }
    }
}
