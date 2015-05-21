using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEZSpiderEventHubProcessor
{
    class FEZSpiderEventHubProcessor : IEventProcessor
    {
        private Stopwatch checkpointStopWatch;

        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine(string.Format("Processor close.  Partition '{0}', Reason: '{1}'.", context.Lease.PartitionId, reason.ToString()));
            if (reason == CloseReason.Shutdown)
            {
                await context.CheckpointAsync();
            }
        }

        public Task OpenAsync(PartitionContext context)
        {
            Console.WriteLine(string.Format("Processor open.  Partition: '{0}', Offset: '{1}'", context.Lease.PartitionId, context.Lease.Offset));
            this.checkpointStopWatch = new Stopwatch();
            this.checkpointStopWatch.Start();
            return Task.FromResult<object>(null);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (EventData eventData in messages)
            {
                if (eventData.Properties.ContainsKey("time"))
                {
                    if (eventData.Properties.ContainsKey("temp"))
                        Console.WriteLine(string.Format("time = {0}, temp = {1}", eventData.Properties["time"], eventData.Properties["temp"]));

                    if (eventData.Properties.ContainsKey("hmdt"))
                        Console.WriteLine(string.Format("time = {0}, hmdt = {1}", eventData.Properties["time"], eventData.Properties["hmdt"]));

                    if (eventData.Properties.ContainsKey("accx") &&
                        eventData.Properties.ContainsKey("accy") &&
                        eventData.Properties.ContainsKey("accz"))
                        Console.WriteLine(string.Format("time = {0}, accx = {1}, accy = {2}, accz = {3}", eventData.Properties["time"], eventData.Properties["accx"], eventData.Properties["accy"], eventData.Properties["accz"]));

                    if (eventData.Properties.ContainsKey("bpm"))
                        Console.WriteLine(string.Format("time = {0}, bpm = {1}", eventData.Properties["time"], eventData.Properties["bpm"]));
                }
            }

            //Call checkpoint every 5 minutes, so that worker can resume processing from the 5 minutes back if it restarts.
            //if (this.checkpointStopWatch.Elapsed > TimeSpan.FromMinutes(5))
            if (this.checkpointStopWatch.Elapsed > TimeSpan.FromSeconds(30))
            {
                await context.CheckpointAsync();
                this.checkpointStopWatch.Restart();
            }
        }
    }
}
