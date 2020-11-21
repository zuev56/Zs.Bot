using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Zs.App.Home.Model.Data;
using Zs.App.Home.Web.Areas.VkAPI.Services;
using Zs.App.Home.Web.Data;
using Zs.Bot.Model.Data;
using BotContextFactory = Zs.Bot.Model.Factories.BotContextFactory;
using HomeContextFactory = Zs.App.Home.Model.ContextFactory;

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

            services.AddDbContext<BotContext>(options =>
                options.UseNpgsql(_configuration.GetConnectionString("Default"))
                       .EnableDetailedErrors(true)
                       .EnableSensitiveDataLogging(true));

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(_configuration.GetConnectionString("Default"))
                       .EnableDetailedErrors(true)
                       .EnableSensitiveDataLogging(true));

            services.AddScoped<IVkActivityService, VkActivityService>(sp =>
            {
                var contextFactory = new HomeContextFactory(
                    sp.GetService<DbContextOptions<BotContext>>(),
                    sp.GetService<DbContextOptions<HomeContext>>());

                return new VkActivityService(contextFactory);
            });

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
                // Кажется, в закомментированных строках нет смысла
                //endpoints.MapControllers();
                
                //endpoints.MapControllerRoute(
                //    name: "AreaRoute",
                //    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                //endpoints.MapRazorPages();
            });

        }
    }
}
