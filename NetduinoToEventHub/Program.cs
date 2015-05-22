using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using ppatierno.TI;
using ppatierno.ST;
using ppatierno.IoT;
using Microsoft.SPOT.Time;
using System.Collections;

namespace NetduinoToEventHub
{
    public class Program
    {
        // TI Sensor Tag parameters
        private byte[] TI_SENSORTAG_ADDR = { 0x4E, 0x58, 0x6E, 0xE5, 0xC5, 0x78 };
#if HEART_RATE
        private byte[] BlueNRG_HRM_ADDR = { 0xFD, 0x00, 0x25, 0xEC, 0x02, 0x04 };

        BlueNRG_HRM blueNGR_HRM;
#endif
        TISensorTag tiSensorTag;
        IIoTClient iotClient;

#if CONNECT_THE_DOTS
        // Event Hub connection string
        private string connectionString = "[EVENT_HUB_CONNECTION_STRING]";
        private string eventhubentity = "[EVENT_HUB_NAME]";
#else
        // Event Hub connection string
        private string connectionString = "[EVENT_HUB_CONNECTION_STRING]";
        private string eventhubentity = "[EVENT_HUB_NAME]";
#endif

        static AutoResetEvent networkAvailableEvent = new AutoResetEvent(false);
        static AutoResetEvent networkAddressChangedEvent = new AutoResetEvent(false);

        public static void Main()
        {
            (new Program()).Run();
        }

        public void Run()
        {
            Microsoft.SPOT.Net.NetworkInformation.NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
            Microsoft.SPOT.Net.NetworkInformation.NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;

            networkAvailableEvent.WaitOne();
            Debug.Print("link is up!");
            networkAddressChangedEvent.WaitOne();
            Debug.Print("address acquired: " + Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].IPAddress);

            Debug.Print("\r\n*** GET NETWORK INTERFACE SETTINGS ***");
            Microsoft.SPOT.Net.NetworkInformation.NetworkInterface[] networkInterfaces = Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            Debug.Print("Found " + networkInterfaces.Length + " network interfaces.");

            // get date/time via NTP
            DateTime dateTime = MFToolkit.Net.Ntp.NtpClient.GetNetworkTime();
            Utility.SetLocalTime(dateTime);

            // set and open the IoT client
            if (this.iotClient == null)
            {
#if CONNECT_THE_DOTS
                    this.iotClient = new IoTClientDots("netduino", Guid.NewGuid().ToString(), connectionString, eventhubentity);
#elif HEART_RATE
                    this.iotClient = new IoTClientHealth("netduino", Guid.NewGuid().ToString(), connectionString, eventhubentity);
#else
                this.iotClient = new IoTClient("netduino", Guid.NewGuid().ToString(), connectionString, eventhubentity);
#endif
            }

            if (!this.iotClient.IsOpen)
                this.iotClient.Open();
            
            // NOTE : seems to be a .Net MF bug when in this ethernet handler and you are waiting
            //        a response on serial port, the data can't be received.
            //        Launch serial communication with BLE module on different thread.
            Thread t = new Thread(this.SensorsSetup);
            t.Start();
            //this.SensorsSetup();

            Thread.Sleep(Timeout.Infinite);
        }

        void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            Debug.Print("NetworkAddressChanged");
            networkAddressChangedEvent.Set();
        }

        void NetworkChange_NetworkAvailabilityChanged(object sender, Microsoft.SPOT.Net.NetworkInformation.NetworkAvailabilityEventArgs e)
        {
            Debug.Print("NetworkAvailabilityChanged " + e.IsAvailable);
            if (e.IsAvailable)
            {
                networkAvailableEvent.Set();
            }
        }

        private void SensorsSetup()
        {
#if HEART_RATE
            BlueNRG_HRMSettings settings =
                new BlueNRG_HRMSettings
                {
                    Address = BlueNRG_HRM_ADDR
                };

            this.blueNGR_HRM = new BlueNRG_HRM(settings);

            this.blueNGR_HRM.SensorValueChanged += device_SensorValueChanged;

            this.blueNGR_HRM.Open();
#else
            // setup TI Sensor Tag
            TISensorTagSettings settings =
                new TISensorTagSettings
                {
                    Address = TI_SENSORTAG_ADDR,
                    IsTemperatureEnabled = true,
                    IsHumidityEnabled = true,
                    IsAccelerometerEnabled = true,
                    Period = 100
                };

            this.tiSensorTag = new TISensorTag(settings);

            // set notification handlers
            this.tiSensorTag.SensorValueChanged += device_SensorValueChanged;

            // open connection and start reading from sensors
            this.tiSensorTag.Open();
#endif
        }

        void device_SensorValueChanged(object sender, IDictionary e)
        {
            if ((this.iotClient != null) && (this.iotClient.IsOpen))
            {
                this.iotClient.SendAsync(e);
            }
        }
    }
}
