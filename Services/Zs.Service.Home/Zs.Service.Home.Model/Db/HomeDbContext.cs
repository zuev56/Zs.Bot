using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Zs.Service.Home.Model.Db
{
    public partial class HomeDbContext : DbContext
    {
        private static DbContextOptions<HomeDbContext> _options;

        public HomeDbContext()
           : base(_options)
        {
        }

        public HomeDbContext(DbContextOptions<HomeDbContext> options)
            : base(options)
        {
        }

        public static void Initialize(DbContextOptions<HomeDbContext> options)
        {
            _options = options;
        }
    }

    //public partial class DbVkVActivityLog
    //{
    //    [Key]
    //    public int DbVkVActivityLogId => (int)ActivityLogId;
    //}
}
