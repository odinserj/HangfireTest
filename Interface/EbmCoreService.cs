using System;
using System.ComponentModel;
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

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CustomQueue : JobFilterAttribute, IElectStateFilter
    {
        public void OnStateElection(ElectStateContext context)
        {
            if (context.CandidateState is EnqueuedState enqueuedState)
            {
                foreach (object arg in context.BackgroundJob.Job.Args)
                {
                    if (arg is CustomJob customJob)
                    {
                        enqueuedState.Queue = customJob.Queue;
                    }
                }
            }
        }
    }

    public class CustomJob
    {
        public CustomJob(string message, string queue, params object[] args)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Queue = queue ?? throw new ArgumentNullException(nameof(queue));
            Args = args ?? throw new ArgumentNullException(nameof(args));
        }

        public string Message { get; }
        public string Queue { get; }
        public object[] Args { get; }
    }
}
