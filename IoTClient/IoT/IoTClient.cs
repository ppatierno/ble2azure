using System;
using System.Collections;
using ppatierno.TI;
#if MF_FRAMEWORK_VERSION_V4_3
using Microsoft.SPOT;
using ppatierno.AzureSBLite.Messaging;
#else
using System.Diagnostics;
using Microsoft.ServiceBus.Messaging;
#endif

namespace ppatierno.IoT
{
    /// <summary>
    /// IoT client implementation
    /// </summary>
    public class IoTClient : IoTClientBase
    {
        public IoTClient(string deviceName, string deviceId, string connectionString, string eventhubentity)
            : base(deviceName, deviceId, connectionString, eventhubentity)
        {
        }

        internal override EventData PrepareEventData(IDictionary bag)
        {
            EventData data = new EventData();

            foreach (SensorType type in bag.Keys)
            {
                if (type == SensorType.Temperature)
                {
                    double temperature = (double)bag[type];
                    data.Properties["time"] = DateTime.UtcNow;
                    data.Properties["temp"] = temperature;
                    Debug.Print("temp: " + temperature);
                }
                else if (type == SensorType.Humidity)
                {
                    double humidity = (double)bag[type];
                    data.Properties["time"] = DateTime.UtcNow;
                    data.Properties["hmdt"] = humidity;
                    Debug.Print("hmdt: " + humidity);
                }
                else if (type == SensorType.Accelerometer)
                {
                    double[] acceleration = (double[])bag[type];
                    data.Properties["time"] = DateTime.UtcNow;
                    data.Properties["accx"] = acceleration[0];
                    data.Properties["accy"] = acceleration[1];
                    data.Properties["accz"] = acceleration[2];
                    Debug.Print("acceleration: " + acceleration[0] + "," + acceleration[1] + "," + acceleration[2]);
                }
            }

            data.PartitionKey = this.DeviceId;

            return data;
        }
    }
}
