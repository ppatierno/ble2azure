using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEZSpiderMonitor
{
    /// <summary>
    /// Object for the charts on UI
    /// </summary>
    class ChartBusinessObject
    {
        public ChartBusinessObjectType Type { get; set; }
        public double Value { get; set; }
        public DateTime Time { get; set; }
    }

    enum ChartBusinessObjectType
    {
        AccelerationX,
        AccelerationY,
        AccelerationZ,
        Humidity,
        Temperature,
        HeartRate
    }
}