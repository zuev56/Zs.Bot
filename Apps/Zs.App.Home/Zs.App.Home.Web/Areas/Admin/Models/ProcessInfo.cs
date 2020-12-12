using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Zs.App.Home.Web.Areas.Admin.Models
{
    public class ProcessInfo
    {
        [Display(Name = "Name")]
        public string Name { get; internal set; }

        [Display(Name = "Responding")]
        public bool IsResponding { get; internal set; }

        [Display(Name = "Threads Number")]
        public int DuplicatesInName { get; internal set; }
        public int ThreadsNumber { get; internal set; }
    }
}
