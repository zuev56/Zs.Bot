using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Zs.App.Home.Web.Areas.App.Models.Vk
{
    public class VkVM
    {
        //[Display(Name = "Name Filter")]
        //public string UserNameFilter { get; set; }
        [Display(Name = "Дата начала")]
        public DateTime FromDate { get; set; }
        [Display(Name = "Дата окончания")]
        public DateTime ToDate { get; set; }



        public List<VkUserVM> VkUsers { get; set; }

        public VkVM()
        {
            FromDate = DateTime.Today - TimeSpan.FromDays(1);
            ToDate = DateTime.Today;
        }
    }
}
