# ble2azure

**IoT Gateway concept from BLE (Bluetooth Low Energy) devices to Azure (Event Hubs)**

This concept is a demo I used for a session at MEC Internet of Things Conference 2015 in Naples.
It shows how to build a simple IoT Gateway from BLE devices to Azure.
The BLE device supported are :

* [TI Sensor Tag](http://www.ti.com/ww/en/wireless_connectivity/sensortag/)
* [STM32 Nucleo Board (with BLE shield)](https://developer.mbed.org/teams/ST-Americas-mbed-Team/code/Nucleo_BLE_HeartRate/wiki/Homepage)

**Projects**

Projects inside the ble2azure solution :

* IoTClient : class that contains the logic to acquire data from the BLE device and sends them to the Azure Event Hubs. It contains a base client for TI Sensor Tag, a Health client for STM32 and a ConnectTheDots client.
* FEZSpiderToEventHub : IoT gateway project with a FEZ Spider board with BLE module by [Innovactive](http://www.innovactive.it/) for acquiring data and sending them to Azure Event Hubs using IoTClient.
* FEZSpiderMonitor : Windows Forms application that uses Telerik chart controls to show data received from Event Hubs (data sent by the gateway).
* FEZSpiderEventHubProcessor : simple console application for an Event Hub processor (without UI).
* FEZSpiderEmulToEventHub : console application for emulating real board (useful for testing)

Other projects needed for this solution :

* Azure SB Lite : http://azuresblite.codeplex.com/
* BLE for .Net MF : https://netmfble.codeplex.com/
* JSON.NetMF : https://github.com/mweimer/Json.NetMF
