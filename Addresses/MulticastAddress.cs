using Kaenx.Konnect.Addresses;
using System;

public class MulticastAddress : IKnxAddress
{
    public MulticastAddress(byte mainGroup, byte middleGroup, byte subGroup)
    {
        MainGroup = mainGroup;
        MiddleGroup = middleGroup;
        SubGroup = subGroup;
    }

    public byte MainGroup { get; }
    public byte MiddleGroup { get; }
    public byte SubGroup { get; }

    public byte[] GetBytes()
    {
        return new[] { (byte)((MainGroup << 3) | MiddleGroup), SubGroup };
    }

    public int AsUInt16()
    {
        byte[] bytes = GetBytes();
        return BitConverter.ToUInt16(new byte[] { bytes[1], bytes[0] }, 0);
    }

    // ── Format-aware string conversion ───────────────────────────────────────

    public string ToString(GroupAddressStyle style) => style switch
    {
        GroupAddressStyle.TwoLevel => $"{MainGroup}/{(MiddleGroup << 8) | SubGroup}",
        GroupAddressStyle.FreeLevel => AsUInt16().ToString(),
        _ => $"{MainGroup}/{MiddleGroup}/{SubGroup}",
    };

    public override string ToString() => ToString(GroupAddressStyle.ThreeLevel);

    // ── Static factories ──────────────────────────────────────────────────────

    public static MulticastAddress FromByteArray(byte[] bytes)
    {
        return new MulticastAddress((byte)(bytes[0] >> 3), (byte)(bytes[0] & 0x07), bytes[1]);
    }

    /// <summary>
    /// Parses a group address string in any style:
    ///   ThreeLevel : "1/2/3"
    ///   TwoLevel   : "1/515"
    ///   FreeLevel  : "1539"
    /// </summary>
    public static MulticastAddress FromString(string address)
    {
        var parts = address.Split('/');

        switch (parts.Length)
        {
            case 3: // ThreeLevel: "1/2/3"
                return new MulticastAddress(
                    Convert.ToByte(parts[0]),
                    Convert.ToByte(parts[1]),
                    Convert.ToByte(parts[2]));

            case 2: // TwoLevel: "1/515"
                {
                    var main = Convert.ToByte(parts[0]);
                    var sub = Convert.ToUInt16(parts[1]); // 0–2047
                    return new MulticastAddress(
                        main,
                        (byte)(sub >> 8),   // Bits 10–8 → MiddleGroup
                        (byte)(sub & 0xFF)); // Bits 7–0  → SubGroup
                }

            case 1: // FreeLevel: "1539"
                {
                    var raw = Convert.ToUInt16(parts[0]);
                    return new MulticastAddress(
                        (byte)(raw >> 11),          // Bits 15–11 → MainGroup
                        (byte)((raw >> 8) & 0x07),  // Bits 10–8  → MiddleGroup
                        (byte)(raw & 0xFF));         // Bits 7–0   → SubGroup
                }

            default:
                throw new Exception($"Invalid group address format: '{address}'");
        }
    }
}

namespace Kaenx.Konnect.Addresses
{
    public enum GroupAddressStyle { ThreeLevel, TwoLevel, FreeLevel }
}