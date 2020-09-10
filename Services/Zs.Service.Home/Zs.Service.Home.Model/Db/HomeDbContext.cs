using Microsoft.EntityFrameworkCore;

namespace Zs.Service.Home.Model.Db
{
    public partial class HomeDbContext : DbContext
    {
        public HomeDbContext(DbContextOptions<HomeDbContext> options)
            : base(options)
        {
        }
    }

    //public partial class DbVkVActivityLog
    //{
    //    [Key]
    //    public int DbVkVActivityLogId => (int)ActivityLogId;
    //}
}
