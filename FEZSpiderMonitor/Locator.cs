using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEZSpiderMonitor
{
    /// <summary>
    /// Locator
    /// </summary>
    class Locator
    {
        /// <summary>
        /// Model for the charts on UI
        /// </summary>
        public LiveDataModel LiveDataModel { get; set; }

        /// <summary>
        /// Heard model for chart on UI
        /// </summary>
        public HeartDataModel HeartDataModel { get; set; }

        /// <summary>
        /// Queue for exchange data between UI and Event Hub Processor
        /// </summary>
        public Queue<ChartBusinessObject> Queue { get; set; }

        // singleton instance
        private static Locator instance;

        /// <summary>
        /// Constructor
        /// </summary>
        private Locator()
        {
            this.LiveDataModel = new LiveDataModel();
            this.HeartDataModel = new HeartDataModel();
            this.Queue = new Queue<ChartBusinessObject>();
        }

        /// <summary>
        /// Get Singleton instance class
        /// </summary>
        /// <returns>Singleton instance class</returns>
        public static Locator GetInstance()
        {
            if (instance == null)
                instance = new Locator();
            return instance;
        }
    }
}
