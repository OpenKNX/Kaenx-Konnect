
![Logo](Assets/Logo.png)
# Kaenx.Konnect
=================

Kaenx.Konnect is a library to connect to a KNX IP Interface.

### Connect to the Interface
```C#
Connection _conn = new Connection(new IPEndPoint(IPAddress.Parse("192.168.0.108"), Convert.ToInt32(3671)));
_conn.Connect();
await Task.Delay(5000);
_conn.Disconnect();
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
