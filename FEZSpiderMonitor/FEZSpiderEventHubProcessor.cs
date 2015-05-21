using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEZSpiderMonitor
{
    class FEZSpiderEventHubProcessor : IEventProcessor
    {
        // queue to exchange data with UI
        private Queue<ChartBusinessObject> queue;

        private Stopwatch checkpointStopWatch;

        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine(string.Format("Processor Shuting Down.  Partition '{0}', Reason: '{1}'.", context.Lease.PartitionId, reason.ToString()));
            if (reason == CloseReason.Shutdown)
            {
                await context.CheckpointAsync();
            }
        }

        public Task OpenAsync(PartitionContext context)
        {
            Debug.WriteLine(string.Format("SimpleEventProcessor initialize.  Partition: '{0}', Offset: '{1}'", context.Lease.PartitionId, context.Lease.Offset));
            this.checkpointStopWatch = new Stopwatch();
            this.checkpointStopWatch.Start();

            this.queue = Locator.GetInstance().Queue;

            return Task.FromResult<object>(null);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (EventData eventData in messages)
            {
                if (eventData.Properties.ContainsKey("time"))
                {
#if HEART_RATE
                    if (eventData.Properties.ContainsKey("bpm"))
                    {
                        queue.Enqueue(this.CreateBusinessObject(ChartBusinessObjectType.HeartRate, (DateTime)eventData.Properties["time"], Convert.ToDouble(eventData.Properties["bpm"])));
                        Debug.WriteLine(string.Format("Partition = {0}, time = {1}, bpm = {2}", context.Lease.PartitionId, eventData.Properties["time"], eventData.Properties["bpm"]));
                    }
#else
                    if (eventData.Properties.ContainsKey("temp"))
                    {
                        queue.Enqueue(this.CreateBusinessObject(ChartBusinessObjectType.Temperature, (DateTime)eventData.Properties["time"], (double)eventData.Properties["temp"]));
                        Debug.WriteLine(string.Format("Partition = {0}, time = {1}, temp = {2}", context.Lease.PartitionId, eventData.Properties["time"], eventData.Properties["temp"]));
                    }

                    if (eventData.Properties.ContainsKey("hmdt"))
                    {
                        queue.Enqueue(this.CreateBusinessObject(ChartBusinessObjectType.Humidity, (DateTime)eventData.Properties["time"], (double)eventData.Properties["hmdt"]));
                        Debug.WriteLine(string.Format("Partition = {0}, time = {1}, hmdt = {2}", context.Lease.PartitionId, eventData.Properties["time"], eventData.Properties["hmdt"]));
                    }
                    
                    if (eventData.Properties.ContainsKey("accx") &&
                        eventData.Properties.ContainsKey("accy") &&
                        eventData.Properties.ContainsKey("accz"))
                    {
                        queue.Enqueue(this.CreateBusinessObject(ChartBusinessObjectType.AccelerationX, (DateTime)eventData.Properties["time"], (double)eventData.Properties["accx"]));
                        queue.Enqueue(this.CreateBusinessObject(ChartBusinessObjectType.AccelerationY, (DateTime)eventData.Properties["time"], (double)eventData.Properties["accy"]));
                        queue.Enqueue(this.CreateBusinessObject(ChartBusinessObjectType.AccelerationZ, (DateTime)eventData.Properties["time"], (double)eventData.Properties["accz"]));
                        
                        Debug.WriteLine(string.Format("Partition = {0}, time = {1}, accx = {2}, accy = {3}, accz = {4}", context.Lease.PartitionId, eventData.Properties["time"], eventData.Properties["accx"], eventData.Properties["accy"], eventData.Properties["accz"]));
                    }
#endif
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

        private ChartBusinessObject CreateBusinessObject(ChartBusinessObjectType type, DateTime date, double value)
        {
            ChartBusinessObject obj = new ChartBusinessObject();

            obj.Type = type;
            obj.Value = value;
            obj.Time = date;

            return obj;
        }
    }
}
