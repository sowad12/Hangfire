using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;

namespace Scheduler
{
    public class SchedulerJobActivator : JobActivator
    {
        private readonly IServiceProvider _serviceProvider;

        public SchedulerJobActivator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override object ActivateJob(Type type)
        {
            return _serviceProvider.GetService(type);
        }
    }
}
