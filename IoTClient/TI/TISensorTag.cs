//#define BLE_NOTIFICATION
//#define BLE_EMULATION

using System;
using Microsoft.SPOT;
using TI_BLE_HCI_ClientLib.Controllers;
using System.Threading;
using System.Collections;

namespace ppatierno.TI
{
    public class TISensorTag
    {
        public delegate void SensorValueChangedEventHandler(object sender, IDictionary e);

        #region Constants ...

        // BLE module parameters
        private const string BLE_SERIAL_PORT = "COM1";
        private const int BLE_BAUD_RATE = 115200;

        // handles for firmware until 1.4
        private const byte DEVICE_NAME_CHARACTERISTIC_HANDLE = 0x0003;

        private const byte IR_TEMP_DATA_CHARACTERISTIC_HANDLE = 0x0025;
        private const byte IR_TEMP_NOTIFICATION_CHARACTERISTIC_HANDLE = 0x0026;
        private const byte IR_TEMP_CONFIG_CHARACTERISTIC_HANDLE = 0x0029;
        
        private const byte HUMIDITY_DATA_CHARACTERISTIC_HANDLE = 0x0038;
        private const byte HUMIDITY_NOTIFICATION_CHARACTERISTIC_HANDLE = 0x0039;
        private const byte HUMIDITY_CONFIG_CHARACTERISTIC_HANDLE = 0x003C;

        private const byte ACCELEROMETER_DATA_CHARACTERISTIC_HANDLE = 0x002D;
        private const byte ACCELEROMETER_NOTIFICATION_CHARACTERISTIC_HANDLE = 0x002E;
        private const byte ACCELEROMETER_CONFIG_CHARACTERISTIC_HANDLE = 0x0031;

        #endregion

        /// <summary>
        /// TI Sensor Tag settings
        /// </summary>
        public TISensorTagSettings Settings { get; private set; }

        // sensors value changed events
        public event SensorValueChangedEventHandler SensorValueChanged;

        // controller and connection handle for BLE module
        private HCISimpleSerialCentralProfileController bleController;
        private ushort bleConnectionHandle;

#if !BLE_NOTIFICATION
        // thread for reading data from sensors
        private Thread readingThread;
        private bool isRunning;
#endif

#if BLE_EMULATION
        Random random = new Random();
#endif

        // sensor values bag
        IDictionary bag;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings">TI Sensor Tag settings</param>
        public TISensorTag(TISensorTagSettings settings)
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
                throw new Exception("No TI Sensor Tag address specified");

            this.bag.Clear();

#if !BLE_EMULATION

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
#endif

#if !BLE_NOTIFICATION
            // using running thread a little connections to get sensors data
            this.isRunning = true;
            this.readingThread = new Thread(this.ReadingThread);
            this.readingThread.Start();
#else
            // using BLE notification
            if (!this.bleController.ConnectTo(this.Settings.Address, out this.bleConnectionHandle))
            {
                Debug.Print("Unable to connect to the TI Sensor Tag");
                return false;
            }

            this.bleController.OnNotification += bleController_OnNotification;

            #region Enable Temperature ...

            if (this.Settings.IsTemperatureEnabled)
            {
                if (!this.bleController.WriteCharacteristic(this.bleConnectionHandle, IR_TEMP_CONFIG_CHARACTERISTIC_HANDLE, (byte)1))
                {
                    Debug.Print("Unable write config 1 on TI Sensor Tag");
                }

                if (!this.bleController.WriteCharacteristic(this.bleConnectionHandle, IR_TEMP_NOTIFICATION_CHARACTERISTIC_HANDLE, (short)1))
                {
                    Debug.Print("Unable write notification 1 on TI Sensor Tag");
                }
            }

            #endregion

            #region Enable Humidity ...

            if (this.Settings.IsHumidityEnabled)
            {
                if (!this.bleController.WriteCharacteristic(this.bleConnectionHandle, HUMIDITY_CONFIG_CHARACTERISTIC_HANDLE, (byte)1))
                {
                    Debug.Print("Unable write config 1 on TI Sensor Tag");
                }

                if (!this.bleController.WriteCharacteristic(this.bleConnectionHandle, HUMIDITY_NOTIFICATION_CHARACTERISTIC_HANDLE, (short)1))
                {
                    Debug.Print("Unable write notification 1 on TI Sensor Tag");
                }
            }

            #endregion

            #region Enable Accelerometer ...

            if (this.Settings.IsAccelerometerEnabled)
            {
                if (!this.bleController.WriteCharacteristic(this.bleConnectionHandle, ACCELEROMETER_CONFIG_CHARACTERISTIC_HANDLE, (byte)1))
                {
                    Debug.Print("Unable write config 1 on TI Sensor Tag");
                }

                if (!this.bleController.WriteCharacteristic(this.bleConnectionHandle, ACCELEROMETER_NOTIFICATION_CHARACTERISTIC_HANDLE, (short)1))
                {
                    Debug.Print("Unable write notification 1 on TI Sensor Tag");
                }
            }

            #endregion
#endif

            return true;
        }

        private void bleController_OnNotification(TI_BLE_HCI_ClientLib.HCIEvents.HCIEvent_ATT_Handle_Value_Notification notification)
        {
            SensorType sensorType = SensorType.Unknown;

            switch (notification.CharacteristicHandle)
            {
                case IR_TEMP_DATA_CHARACTERISTIC_HANDLE:
                    sensorType = SensorType.Temperature;
                    double temperature = this.CalculateAmbientTemperature(notification.Value);
                    if (!bag.Contains(sensorType))
                        bag.Add(sensorType, temperature);
                    else
                        bag[sensorType] = temperature;
                    break;
                case HUMIDITY_DATA_CHARACTERISTIC_HANDLE:
                    sensorType = SensorType.Humidity;
                    double humidity = this.CalculateHumidityInPercent(notification.Value);
                    if (!bag.Contains(sensorType))
                        bag.Add(sensorType, humidity);
                    else
                        bag[sensorType] = humidity;
                    break;
                case ACCELEROMETER_DATA_CHARACTERISTIC_HANDLE:
                    sensorType = SensorType.Accelerometer;
                    sbyte[] signed = { (sbyte)notification.Value[0], (sbyte)notification.Value[1], (sbyte)notification.Value[2] };
                    double[] acceleration = this.CalculateGForce(signed);
                    if (!bag.Contains(sensorType))
                        bag.Add(sensorType, acceleration);
                    else
                        bag[sensorType] = acceleration;
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
#if !BLE_NOTIFICATION
            this.isRunning = false;
            this.readingThread.Join();
#else

            try
            {
                #region Disable Temperature ...

                if (this.Settings.IsTemperatureEnabled)
                {
                    if (!this.bleController.WriteCharacteristic(this.bleConnectionHandle, IR_TEMP_CONFIG_CHARACTERISTIC_HANDLE, (byte)0))
                    {
                        throw new Exception("Unable write config 0 on TI Sensor Tag");
                    }

                    if (!this.bleController.WriteCharacteristic(this.bleConnectionHandle, IR_TEMP_NOTIFICATION_CHARACTERISTIC_HANDLE, (short)0))
                    {
                        throw new Exception("Unable write notification 0 on TI Sensor Tag");
                    }
                }

                #endregion

                #region Disable Humidity ...

                if (this.Settings.IsHumidityEnabled)
                {
                    if (!this.bleController.WriteCharacteristic(this.bleConnectionHandle, HUMIDITY_CONFIG_CHARACTERISTIC_HANDLE, (byte)0))
                    {
                        throw new Exception("Unable write config 0 on TI Sensor Tag");
                    }

                    if (!this.bleController.WriteCharacteristic(this.bleConnectionHandle, HUMIDITY_NOTIFICATION_CHARACTERISTIC_HANDLE, (short)0))
                    {
                        throw new Exception("Unable write notification 0 on TI Sensor Tag");
                    }
                }

                #endregion

                #region Disable Accelerometer ...

                if (this.Settings.IsAccelerometerEnabled)
                {
                    if (!this.bleController.WriteCharacteristic(this.bleConnectionHandle, ACCELEROMETER_CONFIG_CHARACTERISTIC_HANDLE, (byte)0))
                    {
                        throw new Exception("Unable write config 0 on TI Sensor Tag");
                    }

                    if (!this.bleController.WriteCharacteristic(this.bleConnectionHandle, ACCELEROMETER_NOTIFICATION_CHARACTERISTIC_HANDLE, (short)0))
                    {
                        throw new Exception("Unable write notification 0 on TI Sensor Tag");
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
            finally
            {
                if (!this.bleController.CloseConnection(this.bleConnectionHandle))
                {
                    Debug.Print("Unable to disconnect from the TI Sensor Tag");
                }
            }

#endif
            this.bag.Clear();

#if !BLE_EMULATION
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

#if !BLE_NOTIFICATION
        private void ReadingThread()
        {
            byte[] data;

#if BLE_EMULATION
            while (this.isRunning)
            {
                data = null;

                double temperature = this.GetRandomNumber(23, 25);
                if (!bag.Contains(SensorType.Temperature))
                    bag.Add(SensorType.Temperature, temperature);
                else
                    bag[SensorType.Temperature] = temperature;

                Thread.Sleep(1000);

                double humidity = this.GetRandomNumber(40, 50);
                if (!bag.Contains(SensorType.Humidity))
                    bag.Add(SensorType.Humidity, humidity);
                else
                    bag[SensorType.Humidity] = humidity;

                Thread.Sleep(1000);

                double[] acceleration = new double[] { this.GetRandomNumber(-1, 1), this.GetRandomNumber(-1, 1), this.GetRandomNumber(-1, 1) };
                if (!bag.Contains(SensorType.Accelerometer))
                    bag.Add(SensorType.Accelerometer, acceleration);
                else
                    bag[SensorType.Accelerometer] = acceleration;

                if (bag.Count > 0)
                    this.OnSensorValueChanged(bag);

                Thread.Sleep(this.Settings.Period);
            }
#else
            while (this.isRunning)
            {
                data = null;
                
                if (!this.bleController.ConnectTo(this.Settings.Address, out this.bleConnectionHandle))
                {
                    Debug.Print("Unable to connect to the TI Sensor Tag");
                    continue;
                }

                try
                {
                    #region Read Temperature ...

                    if (this.Settings.IsTemperatureEnabled)
                    {
                        if (!this.bleController.WriteCharacteristic(this.bleConnectionHandle, IR_TEMP_CONFIG_CHARACTERISTIC_HANDLE, (byte)1))
                        {
                            throw new Exception("Unable write config 1 on TI Sensor Tag");
                        }

                        Thread.Sleep(1000);

                        if (!this.bleController.ReadCharacteristic(this.bleConnectionHandle, IR_TEMP_DATA_CHARACTERISTIC_HANDLE, out data))
                        {
                            throw new Exception("Unable to read from TI Sensor Tag");
                        }

                        if (!this.bleController.WriteCharacteristic(this.bleConnectionHandle, IR_TEMP_CONFIG_CHARACTERISTIC_HANDLE, (byte)0))
                        {
                            throw new Exception("Unable write config 0 on TI Sensor Tag");
                        }

                        Debug.Print("Temperature read successfully");

                        if (data != null)
                        {
                            double temperature = this.CalculateAmbientTemperature(data);
                            if (!bag.Contains(SensorType.Temperature))
                                bag.Add(SensorType.Temperature, temperature);
                            else
                                bag[SensorType.Temperature] = temperature;
                        }
                    }

                    #endregion

                    #region Read Humidity ...

                    if (this.Settings.IsHumidityEnabled)
                    {
                        if (!this.bleController.WriteCharacteristic(this.bleConnectionHandle, HUMIDITY_CONFIG_CHARACTERISTIC_HANDLE, (byte)1))
                        {
                            throw new Exception("Unable write config 1 on TI Sensor Tag");
                        }

                        Thread.Sleep(1000);

                        if (!this.bleController.ReadCharacteristic(this.bleConnectionHandle, HUMIDITY_DATA_CHARACTERISTIC_HANDLE, out data))
                        {
                            throw new Exception("Unable to read from TI Sensor Tag");
                        }

                        if (!this.bleController.WriteCharacteristic(this.bleConnectionHandle, HUMIDITY_CONFIG_CHARACTERISTIC_HANDLE, (byte)0))
                        {
                            throw new Exception("Unable write config 0 on TI Sensor Tag");
                        }

                        Debug.Print("Humidity read successfully");

                        if (data != null)
                        {
                            double humidity = this.CalculateHumidityInPercent(data);
                            if (!bag.Contains(SensorType.Humidity))
                                bag.Add(SensorType.Humidity, humidity);
                            else
                                bag[SensorType.Humidity] = humidity;
                        }
                    }

                    #endregion

                    #region Read Accelerometer ...

                    if (this.Settings.IsAccelerometerEnabled)
                    {
                        if (!this.bleController.WriteCharacteristic(this.bleConnectionHandle, ACCELEROMETER_CONFIG_CHARACTERISTIC_HANDLE, (byte)1))
                        {
                            throw new Exception("Unable write config 1 on TI Sensor Tag");
                        }

                        Thread.Sleep(1000);

                        if (!this.bleController.ReadCharacteristic(this.bleConnectionHandle, ACCELEROMETER_DATA_CHARACTERISTIC_HANDLE, out data))
                        {
                            throw new Exception("Unable to read from TI Sensor Tag");
                        }

                        if (!this.bleController.WriteCharacteristic(this.bleConnectionHandle, ACCELEROMETER_CONFIG_CHARACTERISTIC_HANDLE, (byte)0))
                        {
                            throw new Exception("Unable write config 0 on TI Sensor Tag");
                        }

                        Debug.Print("Accelerometer read successfully");

                        if (data != null)
                        {
                            sbyte[] signed = { (sbyte)data[0], (sbyte)data[1], (sbyte)data[2] };
                            double[] acceleration = this.CalculateGForce(signed);
                            if (!bag.Contains(SensorType.Accelerometer))
                                bag.Add(SensorType.Accelerometer, acceleration);
                            else
                                bag[SensorType.Accelerometer] = acceleration;
                        }
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                }
                finally
                {
                    if (!this.bleController.CloseConnection(this.bleConnectionHandle))
                    {
                        Debug.Print("Unable to disconnect from the TI Sensor Tag");
                    }
                }

                if (bag.Count > 0)
                    this.OnSensorValueChanged(bag);

                Thread.Sleep(this.Settings.Period);
            }
#endif
        }
#endif

        /// <summary>
        /// Calculates the ambient temperature.
        /// </summary>
        /// <param name="sensorData">Complete array of data retrieved from the sensor</param>
        /// <param name="scale"></param>
        /// <returns>Ambient temperature as double</returns>
        private double CalculateAmbientTemperature(byte[] sensorData)
        {
            return BitConverter.ToUInt16(sensorData, 2) / 128.0;
        }

        /// <summary>
        /// Calculates the humidity in percent.
        /// </summary>
        /// <param name="sensorData">Complete array of data retrieved from the sensor</param>
        /// <returns></returns>
        private double CalculateHumidityInPercent(byte[] sensorData)
        {
            // more info http://www.sensirion.com/nc/en/products/humidity-temperature/download-center/?cid=880&did=102&sechash=c204b9cc
            int hum = BitConverter.ToUInt16(sensorData, 2);

            //cut first two statusbits
            hum = hum - (hum % 4);

            // calculate in percent
            return (-6f) + 125f * (hum / 65535f);
        }

        /// <summary>
        /// Extracts the values of the 3 axis from the raw data of the sensor.
        /// </summary>
        /// <param name="sensorData">Complete array of data retrieved from the sensor</param>
        /// <returns>Array of doubles with the size of 3</returns>
        private double[] CalculateCoordinates(sbyte[] sensorData)
        {
            return CalculateCoordinates(sensorData, 1.0);
        }

        /// <summary>
        /// Extracts the values of the 3 axis from the raw data of the sensor,
        /// </summary>
        /// <param name="sensorData">Complete array of data retrieved from the sensor</param>
        /// <param name="scale">Allows you to scale the accelerometer values</param>
        /// <returns>Array of doubles with the size of 3</returns>
        private double[] CalculateCoordinates(sbyte[] sensorData, double scale)
        {
            if (scale == 0)
                throw new ArgumentOutOfRangeException("scale", "scale cannot be 0");
            return new double[] { sensorData[0] * scale, sensorData[1] * scale, sensorData[2] * scale };
        }

        private double[] CalculateGForce(sbyte[] sensorData)
        {
            return new double[] { (sensorData[0] * 1.0) / (256 / 4), (sensorData[1] * 1.0) / (256 / 4), (sensorData[2] * 1.0) / (256 / 4) };
        }

#if BLE_EMULATION
        private double GetRandomNumber(double minimum, double maximum)
        {
            return this.random.NextDouble() * (maximum - minimum) + minimum;
        }
#endif
    }
}
