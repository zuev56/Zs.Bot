using System;
using Microsoft.EntityFrameworkCore;
using Zs.Common.Interfaces;

namespace Zs.Service.ChatAdmin.DbModel
{
    public partial class ChatAdminDbContext
    {
        private static DbContextOptions<ChatAdminDbContext> _options;
        private static IZsLogger _logger;


        public ChatAdminDbContext()
            : base(_options)
        {
        }

        public static void Initialize(DbContextOptions<ChatAdminDbContext> options)
        {
            if (_options != null)
                throw new InvalidOperationException("Reinitialize is not allowed");

            _options = options;
        }
    }

}
