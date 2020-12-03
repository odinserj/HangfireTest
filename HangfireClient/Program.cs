using Hangfire;
using Hangfire.Client;
using Hangfire.SqlServer;
using Hangfire.States;
using Interface;
using System;
using Hangfire.Common;

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
                    DisableGlobalLocks = true
                });

            JobFilterProviders.Providers.Add(new CustomJobFilterProvider());

            IBackgroundJobClient client = new BackgroundJobClient();
            IState state = new EnqueuedState();

            client.Enqueue<EbmCoreService>(x => x.Execute(
                new CustomJob("business/sleep", new Attribute[] { new QueueAttribute("low") })));

            client.Enqueue<EbmCoreService>(x => x.Execute(
                new CustomJob("business/sleep", new Attribute[] { new QueueAttribute("critical") })));
        }
    }
}
