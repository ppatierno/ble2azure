using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;

namespace FEZSpiderMonitor
{
    /// <summary>
    /// Data model for charts on UI
    /// </summary>
    class HeartDataModel : INotifyPropertyChanged
    {
        // heart rate
        public BindingList<ChartBusinessObject> heartRate;

        public HeartDataModel()
        {
            this.HeartRate = new BindingList<ChartBusinessObject>();
        }

        public BindingList<ChartBusinessObject> HeartRate
        {
            get
            {
                return this.heartRate;
            }
            set
            {
                if (this.heartRate != value)
                {
                    this.heartRate = value;
                    this.OnPropertyChanged("HeartRate");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
