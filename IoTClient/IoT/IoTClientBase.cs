using System;
using System.Collections;
using System.Threading;
#if MF_FRAMEWORK_VERSION_V4_3
using Microsoft.SPOT;
using ppatierno.AzureSBLite.Messaging;
using ppatierno.AzureSBLite;
#else
using System.Diagnostics;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceBus;
#endif

namespace ppatierno.IoT
{
    /// <summary>
    /// Base class for IoT clients
    /// </summary>
    public abstract class IoTClientBase : IIoTClient
    {
        #region Properties ...

        /// <summary>
        /// Client is opened
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// Device name
        /// </summary>
        public string DeviceName { get; private set; }

        /// <summary>
        /// Device ID
        /// </summary>
        public string DeviceId { get; private set; }

        #endregion

        // factory and client for event hub
        private MessagingFactory factory;
        private EventHubClient client;

        // queue to put data and send async
        private Queue queue;
        private AutoResetEvent queueEvent;

        // thread for sending to event hub
        private Thread sendingThread;
        private bool isRunning;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="deviceName">Device name</param>
        /// <param name="deviceId">Device ID</param>
        /// <param name="connectionString">Connection string to Service Bus namespace</param>
        /// <param name="eventhubentity">Event Hub entity name</param>
        public IoTClientBase(string deviceName, string deviceId, string connectionString, string eventhubentity)
        {
            this.DeviceName = deviceName;
            this.DeviceId = deviceId;
            this.queue = new Queue();
            this.queueEvent = new AutoResetEvent(false);

            ServiceBusConnectionStringBuilder builder = new ServiceBusConnectionStringBuilder(connectionString);
            builder.TransportType = TransportType.Amqp;
            this.factory = MessagingFactory.CreateFromConnectionString(builder.ToString());

            this.client = this.factory.CreateEventHubClient(eventhubentity);
        }

        /// <summary>
        /// Open the client
        /// </summary>
        public void Open()
        {
            this.isRunning = true;
            this.sendingThread = new Thread(this.SendingThread);
            this.sendingThread.Start();

            this.IsOpen = true;

#if MF_FRAMEWORK_VERSION_V4_3
            // AMQP.Net Lite tracing
            Amqp.Trace.TraceLevel = Amqp.TraceLevel.Frame | Amqp.TraceLevel.Information;
            Amqp.Trace.TraceListener = (f, a) => Debug.Print(DateTime.Now.ToString("[hh:ss.fff]") + " " + Amqp.Fx.Format(f, a));
#endif
        }

        /// <summary>
        /// Close the client
        /// </summary>
        public void Close()
        {
            this.IsOpen = false;
            this.isRunning = false;
            // unlock reading thread waiting on the queue
            this.queueEvent.Set();
            this.sendingThread.Join();

            this.client.Close();
            this.factory.Close();
        }

        /// <summary>
        /// Thread for sending data
        /// </summary>
        private void SendingThread()
        {
            EventData data = null;

            while (this.isRunning)
            {
                // wait for message in the queue to send
                this.queueEvent.WaitOne();

                if (this.isRunning)
                {
                    lock (this.queue)
                    {
                        data = (this.queue.Count > 0) ? (EventData)this.queue.Dequeue() : null;
                    }

                    if ((data != null) && (this.IsOpen))
                        this.client.Send(data);
                }
            }
        }

        /// <summary>
        /// Send a bag of data in an asynchronous way
        /// </summary>
        /// <param name="bag">Sensor values bag (sensor type - raw data)</param>
        public void SendAsync(IDictionary bag)
        {
            if ((bag != null) && (bag.Count > 0))
            {
                // call method for preparing EventData object (Template Method pattern)
                EventData data = this.PrepareEventData(bag);

                this.Enqueue(data);
            }
        }

        /// <summary>
        /// Enqueue EventData for sending
        /// </summary>
        /// <param name="data">EventData to enqueue</param>
        private void Enqueue(EventData data)
        {
            // enqueue event data
            lock (this.queue)
            {
                this.queue.Enqueue(data);
            }

            this.queueEvent.Set();
        }

        /// <summary>
        /// Prepare the EventData object in the desidered format
        /// </summary>
        /// <param name="bag">Sensor values bag (sensor type - raw data)</param>
        /// <returns>EventData object to send</returns>
        internal abstract EventData PrepareEventData(IDictionary bag);
    }
}
