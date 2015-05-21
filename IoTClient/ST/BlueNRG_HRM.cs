//#define BLE_EMULATION

using System;
using Microsoft.SPOT;
using TI_BLE_HCI_ClientLib.Controllers;
using System.Collections;
using System.Threading;

namespace ppatierno.ST
{
    public class BlueNRG_HRM
    {
        public delegate void SensorValueChangedEventHandler(object sender, IDictionary e);

        #region Constants ...

        // BLE module parameters
        private const string BLE_SERIAL_PORT = "COM1";
        private const int BLE_BAUD_RATE = 115200;

        private const byte HEART_RATE_DATA_CHARACTERISTIC_HANDLE = 0x0012;
        private const byte HEART_RATE_CONFIG_CHARACTERISTIC_HANDLE = 0x0013;
        private const byte BODY_SENSOR_LOCATION_HANDLE = 0x0015;

        #endregion

        /// <summary>
        /// TI Sensor Tag settings
        /// </summary>
        public BlueNRG_HRMSettings Settings { get; private set; }

        // sensors value changed events
        public event SensorValueChangedEventHandler SensorValueChanged;

        // controller and connection handle for BLE module
        private HCISimpleSerialCentralProfileController bleController;
        private ushort bleConnectionHandle;

        // sensor values bag
        IDictionary bag;

#if BLE_EMULATION
        // thread for reading data from sensors
        private Thread readingThread;
        private bool isRunning;
#endif

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings">BlueNRG_HRM settings</param>
        public BlueNRG_HRM(BlueNRG_HRMSettings settings)
        {
            this.Settings = settings;
            this.bag = new Hashtable();
        }

        /// <summary>
        /// Open the BLE controller for connection
        /// </summary>
        /// <returns>Open success</returns>
        public bool Open()
        {
            if (this.Settings.Address == null)
                throw new Exception("No BlueNRG_HRM address specified");

            this.bag.Clear();

#if BLE_EMULATION
            // using running thread simulate sensors data
            this.isRunning = true;
            this.readingThread = new Thread(this.ReadingThread);
            this.readingThread.Start();
#else
            this.bleController = new HCISimpleSerialCentralProfileController(BLE_SERIAL_PORT, BLE_BAUD_RATE);

            this.bleController.Start();

            if (!this.bleController.Reset(HCIController.ResetTypeEnum.HardReset))
            {
                Debug.Print("BLE Controller reset failed");
                return false;
            }

            if (!this.bleController.Init())
            {
                Debug.Print("BLE Controller init failed");
                return false;
            }

            if (!this.bleController.ConnectTo(this.Settings.Address, out this.bleConnectionHandle))
            {
                Debug.Print("Unable to connect to the BlueNRG_HRM");
                return false;
            }

            this.bleController.OnNotification += bleController_OnNotification;

            byte[] data;
            if (!this.bleController.ReadCharacteristic(this.bleConnectionHandle, BODY_SENSOR_LOCATION_HANDLE, out data))
            {
                throw new Exception("Unable to read from BlueNRG_HRM");
            }

            if (!this.bleController.WriteCharacteristic(this.bleConnectionHandle, HEART_RATE_CONFIG_CHARACTERISTIC_HANDLE, (short)1))
            {
                Debug.Print("Unable write notification 1 on BlueNRG_HRM");
            }
#endif

            return true;
        }

        void bleController_OnNotification(TI_BLE_HCI_ClientLib.HCIEvents.HCIEvent_ATT_Handle_Value_Notification notification)
        {
            SensorType sensorType = SensorType.Unknown;

            switch (notification.CharacteristicHandle)
            {
                case HEART_RATE_DATA_CHARACTERISTIC_HANDLE:
                    sensorType = SensorType.HearRate;
                    if (!bag.Contains(sensorType))
                        bag.Add(sensorType, notification.Value[1]);
                    else
                        bag[sensorType] = notification.Value[1];
                    break;
                default:
                    break;
            }

            if (bag.Count > 0)
                this.OnSensorValueChanged(bag);
        }

        /// <summary>
        /// Close the BLE controller for connection
        /// </summary>
        void Close()
        {
            this.bag.Clear();

#if BLE_EMULATION
            this.isRunning = false;
            this.readingThread.Join();
#else
            this.bleController.Terminate();
#endif
        }

        private void OnSensorValueChanged(IDictionary bag)
        {
            if (this.SensorValueChanged != null)
            {
                this.SensorValueChanged(this, bag);
            }
        }

#if BLE_EMULATION

        private void ReadingThread()
        {
            byte[] hrmCounter = { 100 };
            
            while (this.isRunning)
            {
                hrmCounter[0]++;

                if (hrmCounter[0] == 175)
                {
                    hrmCounter[0] = 100;
                }

                SensorType sensorType = SensorType.HearRate;
                if (!bag.Contains(sensorType))
                    bag.Add(sensorType, hrmCounter[0]);
                else
                    bag[sensorType] = hrmCounter[0];

                if (bag.Count > 0)
                    this.OnSensorValueChanged(bag);

                Thread.Sleep(1000);
            }
        }
#endif
    }
}
