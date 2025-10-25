using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Enums
{
    public enum ReturnCodes
    {
        Success = 0,

        // memory cannot be accessed or only with fault(s) 
        MemoryError = 0xF1,

        // Requested data will not fit into a Frame supported by this server.
        LengthExceedsMaxApduLength = 0xF4,

        // This means that one wants to write data beyond what is reserved for the addressed Resource.
        DataOverflow = 0xF5,

        // Write value too low.
        DataMin = 0xF6,

        // Write value too high.
        DataMax = 0xF7,

        // This shall mean that the service or the function (Property) is supported, but the request data is not valid for this receiver.
        DataVoid = 0xF8,

        // This shall mean that the data could in generally be written, but that it is not possible at this time
        TemporarilyNotAvailable = 0xF9,

        // This shall mean that a read access is attempted to a “write only” service or Resource.
        WriteOnly = 0xFA,

        // This shall means that a write access is attempted to a “read only” service or Resource.
        ReadOnly = 0xFB,

        // This shall mean that the access to the data or function is denied because of authorisation reasons, A_Authorize as well as KNX Security.
        AccessDenied = 0xFC,

        // The Interface Object or the Property is not present, or the index is out of range. 
        AddressVoid = 0xFD,

        // Write access with a wrong datatype (Datapoint length)
        DataTypeConflict = 0xFE,

        // The service, function or command has failed without a closer indication of problem.
        Error = 0xFF
    }
}
