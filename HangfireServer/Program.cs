using Hangfire;
using Hangfire.SqlServer;
using Interface;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace HangfireServer
{
    class Program
    {
        static void Main(string[] args)
        {
            
            IServiceCollection services = new ServiceCollection();
            ConfigurePluginAssemblies(services);

            services.AddHangfire(opt => opt
               .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
               .UseColouredConsoleLogProvider()
               .UseSimpleAssemblyNameTypeSerializer()
               .UseIgnoredAssemblyVersionTypeResolver()
               .UseRecommendedSerializerSettings()
              .UseSqlServerStorage(@"Data Source=hsasql12devtxn.reisys.com;database=gems;uid=gemsuser;pwd=gemsuser;", new SqlServerStorageOptions
              {
                  CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                  SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                  QueuePollInterval = TimeSpan.Zero,
                  UseRecommendedIsolationLevel = true,
                  UsePageLocksOnDequeue = true,
                  DisableGlobalLocks = true
              })
              );
            

            using (var server = new BackgroundJobServer())
            {
                Console.WriteLine("Hangfire Server started. Press any key to exit...");
                Console.ReadKey();
            }
        }


        private static void ConfigurePluginAssemblies(IServiceCollection services)
        {
            List<Assembly> assemblies = LoadPlugInAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (typeof(IEbmCoreService).IsAssignableFrom(type))
                    {
                        services.AddTransient(typeof(IEbmCoreService), type);
                    }
                }
            }
        }

        private static List<Assembly> LoadPlugInAssemblies()
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
