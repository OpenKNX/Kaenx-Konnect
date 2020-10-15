
![Logo](Assets/Logo.png)
# Kaenx.Konnect
=================

Kaenx.Konnect is a library to connect to a KNX IP Interface.

### Connect to the Interface via IP
```C#
IKnxConnection _connIp = new KnxIpTunneling(new IPEndPoint(IPAddress.Parse("192.168.0.108"), Convert.ToInt32(3671)));
await _connIp.Connect();
await Task.Delay(5000);
_connUsb.Disconnect();
```

### Connect to the Interface via USB
```C#
IKnxConnection _connUsb = new KnxUsbTunneling(@"\\?\USB\VID_147B&PID_5120\6&16bfd654&0&20"); // USB Device Id
await _connUsb.Connect();
await Task.Delay(5000);
_connUsb.Disconnect();
```


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
dev.Connect();
byte[] data = dev.MemoryRead(16495, 255);
dev.Restart();
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
