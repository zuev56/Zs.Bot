using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Zs.App.Home.Web.Areas.Admin.Models
{
    public class ServerInfo
    {
        public string Name { get; }
        public List<string> IPs { get; }
        public MemoryInfo Memory { get; }
        public List<DriveInfo> Drives { get; } = new List<DriveInfo>();
        public List<ProcessInfo> Processes { get; } = new List<ProcessInfo>();


        public ServerInfo()
        {
            Name = Environment.MachineName;
            IPs = Dns.GetHostAddresses(Dns.GetHostName()).Select(ip => ip.ToString()).ToList();

            foreach (var drive in System.IO.DriveInfo.GetDrives())
            {
                Drives.Add(new DriveInfo
                {
                    Name = drive.Name,
                    Type = drive.DriveType,
                    FileSystem = drive.IsReady ? drive.DriveFormat : "Unknown",
                    TotalSize = drive.IsReady ? drive.TotalSize / (1024 * 1024) : null,
                    FreeSpace = drive.IsReady ? drive.TotalFreeSpace / (1024 * 1024) : null
                });
            }

            var allProcesses = Process.GetProcesses()
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
                Processes.Add(new ProcessInfo
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
