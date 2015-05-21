using System;
using Microsoft.SPOT;

namespace ppatierno.ST
{
    /// <summary>
    /// Settings for BlueNRG_HRM
    /// </summary>
    public class BlueNRG_HRMSettings
    {
        private const int DEFAULT_PERIOD = 10000;

        /// <summary>
        /// TI Sensor Tag BLE address
        /// </summary>
        public byte[] Address { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public BlueNRG_HRMSettings()
        {
            this.Address = null;
        }
    }
}
