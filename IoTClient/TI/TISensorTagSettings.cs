using System;
using Microsoft.SPOT;

namespace ppatierno.TI
{
    /// <summary>
    /// Settings for TI Sensor Tag
    /// </summary>
    public class TISensorTagSettings
    {
        private const int DEFAULT_PERIOD = 10000;

        /// <summary>
        /// TI Sensor Tag BLE address
        /// </summary>
        public byte[] Address { get; set; }

        /// <summary>
        /// Temperature notification enabled
        /// </summary>
        public bool IsTemperatureEnabled { get; set; }

        /// <summary>
        /// Humidity notification enabled
        /// </summary>
        public bool IsHumidityEnabled { get; set; }

        /// <summary>
        /// Accelerometer notification enabled
        /// </summary>
        public bool IsAccelerometerEnabled { get; set; }

        /// <summary>
        /// Reading period for new data from sensors (in ms)
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public TISensorTagSettings()
        {
            this.Address = null;
            this.IsTemperatureEnabled = false;
            this.IsHumidityEnabled = false;
            this.IsAccelerometerEnabled = false;
            this.Period = DEFAULT_PERIOD;
        }
    }
}
