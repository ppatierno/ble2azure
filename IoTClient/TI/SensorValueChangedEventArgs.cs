using System;
using Microsoft.SPOT;

namespace ppatierno.TI
{
    /// <summary>
    /// Event args for sensor value changed event
    /// </summary>
    public class SensorValueChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Raw data bytes 
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Sensor type
        /// </summary>
        public SensorType SensorType { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Raw data bytes</param>
        /// <param name="sensorType">Sensor type</param>
        public SensorValueChangedEventArgs(byte[] data, SensorType sensorType)
        {
            this.Data = data;
            this.SensorType = sensorType;
        }
    }
}
