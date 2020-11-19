using Hangfire;
using Hangfire.Client;
using Hangfire.SqlServer;
using Hangfire.States;
using Interface;
using System;

namespace HangfireClient
{
    class Program
    {
        static void Main(string[] args)
        {
            GlobalConfiguration.Configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseColouredConsoleLogProvider()
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(@"Data Source=$REPLACE", new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    UsePageLocksOnDequeue = true,
                    DisableGlobalLocks = true
                });

     

            IBackgroundJobClient client = new BackgroundJobClient();
            IState state = new EnqueuedState();

            client.Enqueue<Implementation>(x => x.DoWork());

            
            //client.Create<Implementation>(x => x.DoWork(), state);

        }
    }
}
