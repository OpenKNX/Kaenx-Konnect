using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Enums
{
    public enum PropertyDataTypes
    {
        Control = 0x00, // length: 1 read, 10 write
        Char = 0x01, // length: 1
        Unsigned_char = 0x02, // length: 1
        Int = 0x03, // length: 2
        Unsigned_int = 0x04, // length: 2
        Knx_float = 0x05, // length: 2
        Date = 0x06, // length: 3
        Time = 0x07, // length: 3
        Long = 0x08, // length: 4
        Unsigned_long = 0x09, // length: 4
        Float = 0x0a, // length: 4
        Double = 0x0b, // length: 8
        Char_block = 0x0c, // length: 10
        Poll_group_setting = 0x0d, // length: 3
        Short_char_block = 0x0e, // length: 5
        Date_time = 0x0f, // length: 8
        Variable_length = 0x10,
        Generic_01 = 0x11, // length: 1
        Generic_02 = 0x12, // length: 2
        Generic_03 = 0x13, // length: 3
        Generic_04 = 0x14, // length: 4
        Generic_05 = 0x15, // length: 5
        Generic_06 = 0x16, // length: 6
        Generic_07 = 0x17, // length: 7
        Generic_08 = 0x18, // length: 8
        Generic_09 = 0x19, // length: 9
        Generic_10 = 0x1a, // length: 10
        Generic_11 = 0x1b, // length: 11
        Generic_12 = 0x1c, // length: 12
        Generic_13 = 0x1d, // length: 13
        Generic_14 = 0x1e, // length: 14
        Generic_15 = 0x1f, // length: 15
        Generic_16 = 0x20, // length: 16
        Generic_17 = 0x21, // length: 17
        Generic_18 = 0x22, // length: 18
        Generic_19 = 0x23, // length: 19
        Generic_20 = 0x24, // length: 20
        Utf8 = 0x2f, // length: 3
        Version = 0x30, // length: 3
        Alarm_info = 0x31, // length: 3
        Binary_information = 0x32, // length: 3
        Bitset8 = 0x33, // length: 3
        Bitset16 = 0x34, // length: 3
        Enum8 = 0x35, // length: 3
        Scaling = 0x36, // length: 3
        Ne_vl = 0x3c, // length: 3
        Ne_fl = 0x3d, // length: 3
        Function = 0x3e, // length: 3
        Escape = 0x3f  // length: 3
    }
}
