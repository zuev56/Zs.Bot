using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zs.App.Home.Data;
using Zs.App.Home.Data.Models.Vk;
using Zs.App.Home.Services.Vk;
using Zs.Bot.Data.Abstractions;
using Zs.Bot.Data.Repositories;
using Zs.Common.Abstractions;

namespace Zs.App.Home.Web
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<HomeContext>(options =>
                options.UseNpgsql(_configuration.GetConnectionString("Default"))
                       .EnableDetailedErrors(true)
                       .EnableSensitiveDataLogging(true));
            
            services.AddScoped<IContextFactory<HomeContext>, HomeContextFactory>();
            services.AddScoped<IRepository<ActivityLogItem, int>, CommonRepository<HomeContext, ActivityLogItem, int>>();
            services.AddScoped<IRepository<User, int>, CommonRepository<HomeContext, User, int>>();
            services.AddScoped<IActivityService, ActivityService>();
            //services.AddScoped<IVkUserActivityPresenterService, VkUserActivityPresenterService>();

            //services.AddDatabaseDeveloperPageExceptionFilter();
            //services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
            //    .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days.
                // You may want to change this for production scenarios, 
                // see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();


            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // ��� ��������� � ���������
                endpoints.MapControllerRoute(
                    name: "AreaRoute",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                // ����� ����� ��� ��������� � ���� �������� - �������� https://docs.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing?view=aspnetcore-5.0
                endpoints.MapControllers();
                
                // ��� ��������� ��� ��������
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");


                //endpoints.MapRazorPages();
            });

        }
    }
}
