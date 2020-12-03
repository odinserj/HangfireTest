using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Hangfire.Common;
using Hangfire.SqlServer;
using Interface;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Hangfire.Server
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigurePluginAssemblies(services);

            JobFilterProviders.Providers.Add(new CustomJobFilterProvider());

            services.AddHangfire(opt => opt
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseColouredConsoleLogProvider()
            .UseSimpleAssemblyNameTypeSerializer()
            .UseIgnoredAssemblyVersionTypeResolver()
            .UseRecommendedSerializerSettings()
           .UseSqlServerStorage(@"Data Source=$REPLACE", new SqlServerStorageOptions
           {
               CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
               SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
               QueuePollInterval = TimeSpan.Zero,
               UseRecommendedIsolationLevel = true,
               DisableGlobalLocks = true
           })
           );

            services.AddHangfireServer(options =>
                options.Queues = new [] { "critical", "default", "low" });

            services.AddSingleton<IDispatcherRegistry>(provider =>
            {
                var registry = new DispatcherRegistry();

                var dispatchers = provider.GetServices<IDispatcher>();
                foreach (var dispatcher in dispatchers)
                {
                    var attr = dispatcher.GetType().GetCustomAttribute<DispatcherAttribute>();
                    var message = attr != null ? attr.Message : dispatcher.GetType().Name;

                    registry.TryRegister(message, dispatcher);
                }

                return registry;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHangfireDashboard();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }

        private void ConfigurePluginAssemblies(IServiceCollection services)
        {
            List<Assembly> assemblies = LoadPlugInAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (typeof(IDispatcher).IsAssignableFrom(type))
                    {
                        services.AddTransient(typeof(IDispatcher), type);
                    }
                }
            }
        }

        private List<Assembly> LoadPlugInAssemblies()
        {
            List<Assembly> plugInAssemblyList = new List<Assembly>();

            string[] allfiles = Directory.GetFiles("C:/Plugin", "*.*", SearchOption.AllDirectories);
            foreach (var file in allfiles)
            {
                FileInfo info = new FileInfo(file);
                plugInAssemblyList.Add(Assembly.LoadFile(info.FullName));
            }

            return plugInAssemblyList;
        }
    }
}
