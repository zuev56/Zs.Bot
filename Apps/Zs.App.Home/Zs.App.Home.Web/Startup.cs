using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zs.App.Home.Model;
using Zs.App.Home.Model.Data;
using Zs.App.Home.Web.Areas.ApiVk.Services;
using Zs.App.Home.Web.Areas.App.Services;
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
            
            services.AddScoped<IContextFactory<HomeContext>, HomeContextFactory>(sp =>
                new HomeContextFactory(sp.GetService<DbContextOptions<HomeContext>>()));

            services.AddScoped<IRepository<VkActivityLogItem, int>, CommonRepository<HomeContext, VkActivityLogItem, int>>(sp =>
                new CommonRepository<HomeContext, VkActivityLogItem, int>(
                    sp.GetService<IContextFactory<HomeContext>>())
                );

            services.AddScoped<IRepository<VkUser, int>, CommonRepository<HomeContext, VkUser, int>>(sp =>
                new CommonRepository<HomeContext, VkUser, int>(
                    sp.GetService<IContextFactory<HomeContext>>())
                );

            services.AddScoped<IVkActivityService, VkActivityService>(sp =>
                new VkActivityService(
                    sp.GetService<IRepository<VkActivityLogItem, int>>(),
                    sp.GetService<IRepository<VkUser, int>>())
                );
            services.AddScoped<IVkUserActivityPresenterService, VkUserActivityPresenterService>(sp =>
                new VkUserActivityPresenterService(
                    sp.GetService<IRepository<VkActivityLogItem, int>>(),
                    sp.GetService<IRepository<VkUser, int>>())
                );

            //services.AddDatabaseDeveloperPageExceptionFilter();

            //services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
            //    .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddControllersWithViews();

            //services.Configure<ForwardedHeadersOptions>(options =>
            //{
            //    options.KnownProxies.Add(IPAddress.Parse("192.168.1.11"));
            //});
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
                // Для маршрутов с областями
                endpoints.MapControllerRoute(
                    name: "AreaRoute",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                // Вроде нужно для маршрутов в виде атрибута - уточнить https://docs.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing?view=aspnetcore-5.0
                endpoints.MapControllers();
                
                // Для маршрутов без областей
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");


                //endpoints.MapRazorPages();
            });

        }
    }
}
