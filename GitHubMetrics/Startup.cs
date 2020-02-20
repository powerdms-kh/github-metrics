using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Octokit;
using Splitio.Services.Client.Classes;

namespace GitHubMetrics
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddScoped<GitHubClient>(sp =>
            {
                var TokenAuth = new Octokit.Credentials(Configuration["GitHubAccessToken"]);
                var client = new GitHubClient(new ProductHeaderValue("pdms-metrix"));
                client.Credentials = TokenAuth;

                return client;
            });

            services.AddSingleton<Splitio.Services.Client.Interfaces.ISplitClient>(getSplitClient());
        }

        private Splitio.Services.Client.Interfaces.ISplitClient getSplitClient()
        {
            var config = new ConfigurationOptions();
            var factory = new SplitFactory("4m16gd6kbsbu5t4c19m6hpqa49brmjnuo3qj", config);
            var splitClient = factory.Client();

            try
            {
                splitClient.BlockUntilReady(1000);

                return splitClient;
            }
            catch (Exception e)
            {
                throw (e);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
