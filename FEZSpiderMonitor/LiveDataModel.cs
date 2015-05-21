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
    class LiveDataModel : INotifyPropertyChanged
    {
        // temperature
        public BindingList<ChartBusinessObject> temperature;
        // humidity
        public BindingList<ChartBusinessObject> humidity;
        // acceleration (x,y,z)
        public BindingList<ChartBusinessObject> accX;
        public BindingList<ChartBusinessObject> accY;
        public BindingList<ChartBusinessObject> accZ;
 
        public LiveDataModel()
        {
            this.Temperature = new BindingList<ChartBusinessObject>();
            this.Humidity = new BindingList<ChartBusinessObject>();
            this.AccX = new BindingList<ChartBusinessObject>();
            this.AccY = new BindingList<ChartBusinessObject>();
            this.AccZ = new BindingList<ChartBusinessObject>();
        }

        public BindingList<ChartBusinessObject> Temperature
        {
            get
            {
                return this.temperature;
            }
            set
            {
                if (this.temperature != value)
                {
                    this.temperature = value;
                    this.OnPropertyChanged("Temperature");
                }
            }
        }

        public BindingList<ChartBusinessObject> Humidity
        {
            get
            {
                return this.humidity;
            }
            set
            {
                if (this.humidity != value)
                {
                    this.humidity = value;
                    this.OnPropertyChanged("Humidity");
                }
            }
        }

        public BindingList<ChartBusinessObject> AccX
        {
            get
            {
                return this.accX;
            }
            set
            {
                if (this.accX != value)
                {
                    this.accX = value;
                    this.OnPropertyChanged("AccX");
                }
            }
        }

        public BindingList<ChartBusinessObject> AccY
        {
            get
            {
                return this.accY;
            }
            set
            {
                if (this.accY != value)
                {
                    this.accY = value;
                    this.OnPropertyChanged("AccY");
                }
            }
        }

        public BindingList<ChartBusinessObject> AccZ
        {
            get
            {
                return this.accZ;
            }
            set
            {
                if (this.accZ != value)
                {
                    this.accZ = value;
                    this.OnPropertyChanged("AccZ");
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
