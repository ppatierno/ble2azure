using ppatierno.IoT;
using ppatierno.TI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FEZSpiderEmulToEventHub
{
    class Program
    {
        Random random = new Random();

        static void Main(string[] args)
        {
            (new Program()).Run();
        }

        void Run()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = tokenSource.Token;

            var task = Task.Run(() =>
                {
                    // already canceled
                    cancellationToken.ThrowIfCancellationRequested();

                    string connectionString = ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
                    string eventhubentity = ConfigurationManager.AppSettings["EventHubName"];

                    IIoTClient iotClient = new IoTClient("emulgadgeteer", Guid.NewGuid().ToString(), connectionString, eventhubentity);
                    iotClient.Open();

                    IDictionary bag = new Hashtable();
                    
                    while (true)
                    {
                        bag.Clear();
                        bag.Add(SensorType.Temperature, this.GetRandomNumber(23, 25));
                        bag.Add(SensorType.Humidity, this.GetRandomNumber(40, 50));
                        bag.Add(SensorType.Accelerometer, new double[] { this.GetRandomNumber(-1, 1), this.GetRandomNumber(-1, 1), this.GetRandomNumber(-1, 1) });
                        
                        iotClient.SendAsync(bag);

                        // check if cancellation requested
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        Thread.Sleep(1000);
                    }

                    iotClient.Close();

                }, cancellationToken);

            Console.WriteLine("Click a button to end the emulator ...");
            Console.ReadLine();

            tokenSource.Cancel();

            try
            {
                task.Wait();
            }
            catch (AggregateException e)
            {
                foreach (var v in e.InnerExceptions)
                    Console.WriteLine(e.Message + " " + v.Message);
            }
            finally
            {
                tokenSource.Dispose();
            }
        }

        private double GetRandomNumber(double minimum, double maximum)
        {
            return this.random.NextDouble() * (maximum - minimum) + minimum;
        }
    }
}
