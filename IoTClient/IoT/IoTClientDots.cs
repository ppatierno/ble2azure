using System;
using System.Collections;
using Json.NETMF;
using ppatierno.TI;
#if MF_FRAMEWORK_VERSION_V4_3
using Microsoft.SPOT;
using ppatierno.AzureSBLite.Messaging;
#endif

namespace ppatierno.IoT
{
    /// <summary>
    /// IoT client implementation for the ConnectTheDots project
    /// </summary>
    public class IoTClientDots : IoTClientBase
    {   
        public IoTClientDots(string deviceName, string deviceId, string connectionString, string eventhubentity)
            : base(deviceName, deviceId, connectionString, eventhubentity)
        {
        }

        internal override EventData PrepareEventData(IDictionary bag)
        {
            EventData data = new EventData();

            // Create hashtable for data
            Hashtable hashtable = new Hashtable();
            hashtable.Add("Subject", "wthr");
            hashtable.Add("time", DateTime.UtcNow);
            hashtable.Add("from", this.DeviceId);
            hashtable.Add("dspl", this.DeviceName);

            foreach (SensorType type in bag.Keys)
            {
                if (type == SensorType.Temperature)
                {
                    double temperature = (double)bag[type];
                    data.Properties["temp"] = temperature;
                    Debug.Print("temp: " + temperature);
                }
                else if (type == SensorType.Humidity)
                {
                    double humidity = (double)bag[type];
                    data.Properties["hmdt"] = humidity;
                    Debug.Print("hmdt: " + humidity);
                }
                else if (type == SensorType.Accelerometer)
                {
                    double[] acceleration = (double[])bag[type];
                    data.Properties["accx"] = acceleration[0];
                    data.Properties["accy"] = acceleration[1];
                    data.Properties["accz"] = acceleration[2];
                    Debug.Print("acceleration: " + acceleration[0] + "," + acceleration[1] + "," + acceleration[2]);
                }
            }

            // Serialize hashtable into JSON
            JsonSerializer serializer = new JsonSerializer(DateTimeFormat.Default);
            string payload = serializer.Serialize(hashtable);

            data.PartitionKey = this.DeviceId;

            return data;
        }
    }
}
