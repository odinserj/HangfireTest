using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
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

            services.AddHangfireServer();

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
                var loadContext = new PluginLoadContext(info.FullName);

                var assembly = loadContext.LoadFromAssemblyName(
                    new AssemblyName(
                        Path.GetFileNameWithoutExtension(info.FullName)));

                plugInAssemblyList.Add(assembly);
            }

            return plugInAssemblyList;
        }
    }

    class PluginLoadContext : AssemblyLoadContext
    {
        private AssemblyDependencyResolver _resolver;

        public PluginLoadContext(string pluginPath)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }
    }
}
