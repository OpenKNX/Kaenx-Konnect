![Logo](Kaenx.Konnect/Assets/Logo.png)
# Kaenx.Konnect
=================

[Nuget Package](https://www.nuget.org/packages/Kaenx.Konnect/) is availible.

Kaenx.Konnect is a library to connect to a KNX IP Interface.

### Connect to the Interface via IP
```C#
IKnxConnection _connIp = new KnxIpTunneling(new IPEndPoint(IPAddress.Parse("192.168.0.108"), Convert.ToInt32(3671)));
_connIp = new KnxIpTunneling("192.168.0.108", 3671);
await _connIp.Connect();
await Task.Delay(5000);
_connIp.Disconnect();
```


### Search for KNX IP Interfaces
```C#
IKnxConnection _connIp = new KnxIpTunneling("224.0.23.12", 3671, true); //Use sendBroadcast to send Searchrequest to all Network Interfaces on the PC
_connIp.Send(new MsgSearchReq(), true); // true to ignore if connection ist connected
```


### Connect to the Interface via USB
```C#
ConnectedDeviceDefinition def; //See Device.Net on how to get ConnectedDeviceDefinition
IKnxConnection _connUsb = new KnxUsbTunneling(def);
await _connUsb.Connect();
await Task.Delay(5000);
_connUsb.Disconnect();
```
Connection via USB is not widly implemented yet.


### Connection Events
There are three Events:
- OnTunnelRequest: 
  Is Invoked when the Interface receives a Request. (GroupValueWrite, IndividualAddressRead, etc.)
- OnTunnelResponse: 
  Is Invoked when the Interface receives a Response. (GroupValueReadResponse, MemoryReadResponse, etc.)
- OnAck:
  Is Invoked when the Interface receives an Ack.
  
  
### Bus Device
Create a Bus Device to read Property or Memory from it. Also you can restart it.
```C#
BusDevice dev = new BusDevice("1.1.2", _conn);
await dev.Connect();
byte[] data = await dev.MemoryRead(16495, 255);
string mask = await dev.DeviceDescriptorRead(); //Returns Mask Version like 0701, 07B0, ...
string serial = await dev.PropertyRead<string>("MV-" + mask, "DeviceSerialNumber"); //Returns SerialNumber of Device
await dev.Restart();
dev.Disconnect();
```

### Bus Common
Use this Class for common tasks on the bus like IndividualAddressRead or GroupValueWrite.
```C#
BusCommon bus = new BusCommon(_conn);
bus.IndividualAddressWrite(UnicastAddress.FromString("1.1.6"));
bus.GroupValueWrite(MulticastAddress.FromString("1/4/3"), 0x1);
```

# Credits

Many thanks go to @[xp-development](https://github.com/xp-development) and his Code from [Automation.KNX](https://github.com/xp-development/Automation.Knx) which was the base code for my lib.
