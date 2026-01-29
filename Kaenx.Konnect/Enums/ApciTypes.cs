using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Enums
{
    public enum ApciTypes
    {
        Undefined = -1,
        GroupValueRead = 0x000,
        GroupValueResponse = 0x040,
        GroupValueWrite = 0x080,
        IndividualAddressWrite = 0x0C0,

        IndividualAddressRead = 0x100,
        IndividualAddressResponse = 0x140,
        ADCRead = 0x180,
        ADCResponse = 0x1C0,
        //
        SystemNetworkParameterRead = 0x1C8,
        SystemNetworkParameterResponse = 0x1C9,
        SystemNetworkParameterWrite = 0x1CA,
        // reserved 0x1CB
        PropertyExtendedValueRead = 0x1CC,
        PropertyExtendedValueResponse = 0x1CD,
        PropertyExtendedValueWriteConfirm = 0x1CE,
        PropertyExtendedValueWriteConfirmResponse = 0x1CF,
        PropertyExtendedValueWriteUnconfirm = 0x1D0,
        PropertyExtendedValueInfoReport = 0x1D1,
        PropertyExtendedDescriptionRead = 0x1D2,
        PropertyExtendedDescriptionResponse = 0x1D3,
        FunctionPropertyExtendedCommand = 0x1D4,
        FunctionPropertyExtendedStateRead = 0x1D5,
        FunctionPropertyExtendedStateResponse = 0x1D6,
        //
        MemoryExtendedRead = 0x1FB,
        MemoryExtendedReadResponse = 0x1FC,
        MemoryExtendedWrite = 0x1FD,
        MemoryExtendedWriteResponse = 0x1FE,


        MemoryRead = 0x200,
        MemoryResponse = 0x240,
        MemoryWrite = 0x280,
        //
        UserMemoryRead = 0x2C0,
        UserMemoryResponse = 0x2C1,
        UserMemoryWrite = 0x2C2,
        //
        UserMemoryBitWrite = 0x2C4,
        UserManufacturerInfoRead = 0x2C5,
        UserManufacturerInfoResponse = 0x2C6,
        FunctionPropertyCommand = 0x2C7,
        FunctionPropertyStateRead = 0x2C8,
        FunctionPropertyStateResponse = 0x2C9,

        // 0x2CA - 0x2F7 reserved usermsg
        // 0x2F8 - 0x2FE reserved manufacturer usermsg

        DeviceDescriptorRead = 0x300,
        DeviceDescriptorResponse = 0x340,
        Restart = 0x380,
        FilterTableOpen = 0x3C0,
        FilterTableRead = 0x3C1,
        FilterTableResponse = 0x3C2,
        FilterTableWrite = 0x3C3,
        RouterMemoryRead = 0x3C8,
        RouterMemoryReadResponse = 0x3C9,
        RouterMemoryWrite = 0x3CA,
        RouterStatusRead = 0x3CD,
        RouterStatusReadResponse = 0x3CE,
        RouterStatusWrite = 0x3CF,
        MemoryBitWrite = 0x3D0, // not for future use
        AuthorizeRequest = 0x3D1,
        AuthorizeResponse = 0x3D2,
        KeyWrite = 0x3D3,
        KeyResponse = 0x3D4,
        PropertyValueRead = 0x3D5,
        PropertyValueResponse = 0x3D6,
        PropertyValueWrite = 0x3D7,
        PropertyDescriptionRead = 0x3D8,
        PropertyDescriptionResponse = 0x3D9,
        NetworkParameterRead = 0x3DA,
        NetworkParameterResponse = 0x3DB,
        IndividualAddressSerialNumberRead = 0x3DC,
        IndividualAddressSerialNumberResponse = 0x3DD,
        IndividualAddressSerialNumberWrite = 0x3DE,
        // reserved 0x3DF
        DomainAddressWrite = 0x3E0,
        DomainAddressRead = 0x3E1,
        DomainAddressResponse = 0x3E2,
        DomainAddressSelectiveRead = 0x3E3,
        NetworkParameterWrite = 0x3E4,
        LinkRead = 0x3E5,
        LinkResponse = 0x3E6,
        LinkWrite = 0x3E7,
        GroupPropValueRead = 0x3E8,
        GroupPropValueResponse = 0x3E9,
        GroupPropValueWrite = 0x3EA,
        GroupPropValueInfoReport = 0x3EB,
        DomainAddressSerialNumberRead = 0x3EC,
        DomainAddressSerialNumberResponse = 0x3ED,
        DomainAddressSerialNumberWrite = 0x3EE,
        FileStreamInfoReport = 0x3F0,


        DataSecure = 0x3F1,

        Connect = 0x8000,
        Disconnect = 0x8100,
        Ack = 0x8200, //TODO remove last byte
        NAK = 0x8300, //TODO remove last byte
    }
}
