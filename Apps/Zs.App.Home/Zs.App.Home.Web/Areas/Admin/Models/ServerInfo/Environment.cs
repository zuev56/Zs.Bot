using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;

namespace Zs.App.Home.Web.Areas.Admin.Models.ServerInfo
{
    public class Environment
    {
        [Display(Name = "Machine Name")]
        public string MachineName => System.Environment.MachineName;
        [Display(Name = "Current Date")]
        public DateTime CurrentDateGMT => DateTime.Now;
        [Display(Name = "Current Date UTC")]
        public DateTime CurrentDateUTC => DateTime.UtcNow;
        [Display(Name = "IP addresses")]
        public List<string> IPs => Dns.GetHostAddresses(Dns.GetHostName()).Select(ip => ip.ToString()).ToList();
        [Display(Name = "OS Version")]
        public string OSVersion => System.Environment.OSVersion.VersionString;
        [Display(Name = "Running")]
        public string Running => GetRunningTime();
        [Display(Name = "Application Directory")]
        public string CurrentDirectory => System.Environment.CurrentDirectory;
        [Display(Name = "System Directory")]
        public string SystemDirectory => System.Environment.SystemDirectory;
        public string StackTrace => System.Environment.StackTrace;
        [Display(Name = "Processor Count")]
        public int CpuCount => System.Environment.ProcessorCount;

        [Display(Name = "Runtime Framework")]
        public string RuntimeFramework => RuntimeInformation.FrameworkDescription;

        [Display(Name = "Runtime OS")]
        public string RuntimeOS => RuntimeInformation.OSDescription;
        [Display(Name = "Runtime ID")]
        public string RuntimeId => RuntimeInformation.RuntimeIdentifier;
        [Display(Name = "Process Architecture")]
        public string RuntimeProcessArchitecture => RuntimeInformation.ProcessArchitecture.ToString();
        [Display(Name = "OS Architecture")]
        public string RuntimeOSArchitecture => RuntimeInformation.OSArchitecture.ToString();

        private string GetRunningTime()
        {
            var ts = TimeSpan.FromMilliseconds(System.Environment.TickCount64);
            return $"{(int)ts.TotalDays} days {ts.Hours}:{ts.Minutes}:{ts.Seconds}";
        }

    }
}
