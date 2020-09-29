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

            services.AddSingleton<IContextFactory<HomeContext>, HomeContextFactory>(sp =>
                new HomeContextFactory(sp.GetService<DbContextOptions<HomeContext>>()));

            services.AddSingleton<IVkActivityService, VkActivityService>();
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
