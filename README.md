# ble2azure

**IoT Gateway concept from BLE (Bluetooth Low Energy) devices to Azure (Event Hubs)**

This concept is a demo I used for a session at MEC Internet of Things Conference 2015 in Naples.
It shows how to build a simple IoT Gateway from BLE devices to Azure.
The BLE devices supported are :

* [TI Sensor Tag](http://www.ti.com/ww/en/wireless_connectivity/sensortag/) : provides temperature, humidity, accelerometer and other useful information
* [STM32 Nucleo Board (with BLE shield)](https://developer.mbed.org/teams/ST-Americas-mbed-Team/code/Nucleo_BLE_HeartRate/wiki/Homepage) : using a firmware that emulates a standard BLE HRM (Heart Rate Monitoring) device

**Projects**

Projects inside the ble2azure solution :

* **IoTClient** : class that contains the logic to acquire data from the BLE device and sends them to the Azure Event Hubs. It contains a base client for TI Sensor Tag, a Health client for STM32 and a [ConnectTheDots](https://github.com/MSOpenTech/connectthedots) client (it's like tha base client but sends information in JSON format useful to the ConnectTheDots project).
* **FEZSpiderToEventHub** : IoT gateway .Net Gadgeteer based project (.Net Micro Framework) with a FEZ Spider board with BLE module by [Innovactive](http://www.innovactive.it/) - [Lorenzo Maiorfi](http://mvp.microsoft.com/en-us/mvp/Lorenzo%20Maiorfi-5000212) for acquiring data and sending them to Azure Event Hubs using the IoTClient.
* **FEZSpiderMonitor** : Windows Forms application that acquires data from Event Hubs (data sent by the gateway) using Event Hub Processor and uses [Telerik](http://www.telerik.com/) chart controls to show them.
* **FEZSpiderEventHubProcessor** : simple console application that used an Event Hub Processor to acquire data from Event Hubs (data sent by the gateway). It's like the previous monitor but without a UI.
* **FEZSpiderEmulToEventHub** : console application for emulating the real board IoT gateway (useful for testing and sending data from a PC)

Other projects needed for this solution :

* [Azure SB Lite](http://azuresblite.codeplex.com/) : library for connecting to the Azure Service Bus services (Queues, Topics/Subscriptions and Event Hubs) using AMQP protocol. It's based on [AMQP .Net Lite library](http://amqpnetlite.codeplex.com/)
* [BLE for .Net MF](https://netmfble.codeplex.com/) : BLE Class Library for BLE (aka Bluetooth Low Energy, aka Bluetooth 4.0) support targeted to .NET Micro Framework
* [JSON.NetMF](https://github.com/mweimer/Json.NetMF) : library for parsing JSON
