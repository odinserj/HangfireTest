using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Hangfire.Common;
using Hangfire.States;

namespace Interface
{
    public class EbmCoreService
    {
        private readonly IDispatcherRegistry _registry;

        public EbmCoreService(IDispatcherRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        [DisplayName("EBM ({0})")]
        public void Execute(CustomJob job)
        {
            var dispatcher = _registry.FindDispatcher(job.Message);
            dispatcher.Dispatch(job.Args);
        }
    }

    public class CustomJobFilterProvider : IJobFilterProvider
    {
        public IEnumerable<JobFilter> GetFilters(Job job)
        {
            foreach (var arg in job.Args)
            {
                if (arg is CustomJob customJob)
                {
                    foreach (var filter in customJob.Filters)
                    {
                        var jobFilter = filter as JobFilterAttribute;

                        yield return new JobFilter(
                            filter,
                            JobFilterScope.Method,
                            jobFilter?.Order);
                    }
                }
            }
        }
    }

    public class CustomJob
    {
        public CustomJob(string message, Attribute[] filters, params object[] args)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Filters = filters ?? throw new ArgumentNullException(nameof(filters));
            Args = args ?? throw new ArgumentNullException(nameof(args));
        }

        public string Message { get; }
        public Attribute[] Filters { get; }
        public object[] Args { get; }
    }
}
