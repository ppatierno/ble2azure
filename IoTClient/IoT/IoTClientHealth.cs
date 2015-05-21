using System;
using System.Collections;
using ppatierno.ST;
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
    public class IoTClientHealth : IoTClientBase
    {
        public IoTClientHealth(string deviceName, string deviceId, string connectionString, string eventhubentity)
            : base(deviceName, deviceId, connectionString, eventhubentity)
        {
        }

        internal override EventData PrepareEventData(IDictionary bag)
        {
            EventData data = new EventData();

            foreach (SensorType type in bag.Keys)
            {
                if (type == SensorType.HearRate)
                {
                    byte bpm = (byte)bag[type];
                    data.Properties["time"] = DateTime.UtcNow;
                    data.Properties["bpm"] = bpm;
                    Debug.Print("bpm: " + bpm);
                }
            }

            data.PartitionKey = this.DeviceId;

            return data;
        }
    }
}
