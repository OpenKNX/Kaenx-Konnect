
public enum ServiceFamilyTypes
{
    Core = 2,
    DeviceManagement,
    Tunneling,
    Routing,
    Unknown = 256
}

public class ServiceFamily
{
    public ServiceFamilyTypes ServiceFamilyType { get; set; } = ServiceFamilyTypes.Unknown;
    public int Version { get; set; } = 0;
}