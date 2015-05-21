using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;
using ppatierno.TI;
using ppatierno.IoT;
using Microsoft.SPOT.Time;
using System.Net;
using ppatierno.ST;
using Microsoft.SPOT.Net.NetworkInformation;

namespace FEZSpiderToEventHub
{
    public partial class Program
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

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            /*******************************************************************************************
            Modules added in the Program.gadgeteer designer view are used by typing 
            their name followed by a period, e.g.  button.  or  camera.
            
            Many modules generate useful events. Type +=<tab><tab> to add a handler to an event, e.g.:
                button.ButtonPressed +=<tab><tab>
            
            If you want to do something periodically, use a GT.Timer and handle its Tick event, e.g.:
                GT.Timer timer = new GT.Timer(1000); // every second (1000ms)
                timer.Tick +=<tab><tab>
                timer.Start();
            *******************************************************************************************/


            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");

            this.HardwareSetup();

            NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
            NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;

            this.EthernetSetup();
        }

        private void HardwareSetup()
        {
            this.characterDisplay.Clear();

            this.characterDisplay.CursorHome();
            this.characterDisplay.Print("Startup...");
        }

        private void WriteOnDisplay(int row, int column, string text)
        {
            this.characterDisplay.SetCursorPosition(row, column);
            this.characterDisplay.Print(new string(' ', 16));
            this.characterDisplay.SetCursorPosition(row, column);
            this.characterDisplay.Print(text);
        }

        void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            Debug.Print("network " + (e.IsAvailable ? "is available" : "isn't Available"));
        }

        void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            Debug.Print("Network Address Change to: " + this.ethernetJ11D.NetworkInterface.IPAddress);
            if (this.ethernetJ11D.NetworkInterface.IPAddress != "0.0.0.0")
            {
                Debug.Print("IP Address: " + this.ethernetJ11D.NetworkInterface.IPAddress);
                Debug.Print("Gateway Address: " + this.ethernetJ11D.NetworkInterface.GatewayAddress);
                foreach (string dns in this.ethernetJ11D.NetworkInterface.DnsAddresses)
                {
                    Debug.Print("DNS Address: " + dns);
                }

                this.WriteOnDisplay(0,0, "IP Address:");
                this.WriteOnDisplay(1,0, this.ethernetJ11D.NetworkInterface.IPAddress);

                try
                {
                    this.TimeServiceSetup();
                }
                catch (Exception)
                {
                    Debug.Print("TimeService initialization failed");
                    // fake time for debugging ?
                    TimeService.SetUtcTime((new DateTime(2015, 4, 18, 11, 30, 00)).Ticks);
                    Debug.Print("DateTime = " + DateTime.Now.ToString());
                }

                // set and open the IoT client
                if (this.iotClient == null)
                {
#if CONNECT_THE_DOTS
                    this.iotClient = new IoTClientDots("gadgeteer", Guid.NewGuid().ToString(), connectionString, eventhubentity);
#elif HEART_RATE
                    this.iotClient = new IoTClientHealth("gadgeteer", Guid.NewGuid().ToString(), connectionString, eventhubentity);
#else
                    this.iotClient = new IoTClient("gadgeteer", Guid.NewGuid().ToString(), connectionString, eventhubentity);
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
            }
        }

        private void EthernetSetup()
        {
            // DHCP
            /*
            this.ethernetJ11D.NetworkInterface.EnableDhcp();

            this.ethernetJ11D.NetworkInterface.EnableDynamicDns();

            this.ethernetJ11D.NetworkInterface.Open();
            */
            // STATIC

            
            this.ethernetJ11D.NetworkInterface.Open();

            // HOME
            //this.ethernetJ11D.UseStaticIP("192.168.1.203", "255.255.255.0", "192.168.1.254");
            //this.ethernetJ11D.NetworkInterface.EnableStaticDns(new string[] { "192.168.1.254", "8.8.8.8" });
            // WORK
            this.ethernetJ11D.UseStaticIP("192.168.1.203", "255.255.255.0", "192.168.1.1");
            this.ethernetJ11D.NetworkInterface.EnableStaticDns(new string[] { "8.8.8.8", "8.8.4.4" });
            // TETHERING
            //this.ethernetJ11D.UseStaticIP("192.168.137.40", "255.255.255.0", "192.168.137.1");
            //this.ethernetJ11D.NetworkInterface.EnableStaticDns(new string[] { "192.168.137.1", "8.8.8.8" });           
            
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

        private void TimeServiceSetup()
        {
            // open time service for SNTP synchronization
            TimeServiceSettings settings = new TimeServiceSettings();
            settings.RefreshTime = 600; // every 600 seconds (10 minutes)
            settings.ForceSyncAtWakeUp = true;

            TimeService.SystemTimeChanged += TimeService_SystemTimeChanged;
            TimeService.TimeSyncFailed += TimeService_TimeSyncFailed;
            TimeService.SetTimeZoneOffset(60);

            IPHostEntry hostEntry = Dns.GetHostEntry("time.nist.gov");
            IPAddress[] address = hostEntry.AddressList;
            if (address != null)
                settings.PrimaryServer = address[0].GetAddressBytes();

            hostEntry = Dns.GetHostEntry("time.windows.com");
            address = hostEntry.AddressList;
            if (address != null)
                settings.AlternateServer = address[0].GetAddressBytes();

            TimeService.Settings = settings;
            TimeService.Start();
        }

        void TimeService_TimeSyncFailed(object sender, TimeSyncFailedEventArgs e)
        {
            Debug.Print("DateTime Sync Failed");
        }

        void TimeService_SystemTimeChanged(object sender, SystemTimeChangedEventArgs e)
        {
            Debug.Print("DateTime = " + DateTime.Now.ToString());
            this.WriteOnDisplay(0, 0, "DateTime:");
            this.WriteOnDisplay(1, 0, DateTime.Now.ToString());
        }
    }
}
