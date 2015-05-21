using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEZSpiderEventHubProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            string eventHubConnectionString = ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
            string eventHubName = ConfigurationManager.AppSettings["EventHubName"];
            string storageConnectionString = ConfigurationManager.AppSettings["Microsoft.WindowsAzure.Storage.ConnectionString"];

            string eventProcessorHostName = Guid.NewGuid().ToString();
            EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, eventHubName, EventHubConsumerGroup.DefaultGroupName, eventHubConnectionString, storageConnectionString);
            eventProcessorHost.RegisterEventProcessorAsync<FEZSpiderEventHubProcessor>().Wait();

            Console.WriteLine("Receiving. Press enter key to stop worker.");
            Console.ReadLine();
        }
    }
}
