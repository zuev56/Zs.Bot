using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Zs.Bot.Model.Db;

namespace MyTestWebInterface.Models
{
    public class UserRoleViewModel
    {
        public List<DbUser> Users { get; set; }
        public SelectList RoleCodes { get; set; }
        public string UserRoleCode { get; set; }
        public string SearchString { get; set; }
    }
}
