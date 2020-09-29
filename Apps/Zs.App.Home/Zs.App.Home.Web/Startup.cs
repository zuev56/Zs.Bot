using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zs.App.Home.Model.Data;
using Zs.App.Home.Model;
using Zs.App.Home.Web.Services;
using Zs.Common.Abstractions;
using Zs.Bot.Model.Data;
using Zs.App.Home.Model.Abstractions;
using Zs.Bot.Model.Factories;
using BotContextFactory = Zs.Bot.Model.Factories.BotContextFactory;
using HomeContextFactory = Zs.App.Home.Model.ContextFactory;

namespace Zs.App.Home.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddDbContext<HomeContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("Default"))
                       .EnableDetailedErrors(true)
                       .EnableSensitiveDataLogging(true));

            services.AddDbContext<BotContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("Default"))
                       .EnableDetailedErrors(true)
                       .EnableSensitiveDataLogging(true));

            services.AddScoped<IVkActivityService, VkActivityService>(sp => 
            {
                var contextFactory = new HomeContextFactory(
                    sp.GetService<DbContextOptions<BotContext>>(),
                    sp.GetService<DbContextOptions<HomeContext>>());

                return new VkActivityService(contextFactory);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            //app.UseExceptionHandler("/Error");

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
