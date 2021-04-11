using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Kaenx.Konnect.Metadata
{
    /// <summary>
    /// Raw KNX MetaData 
    /// </summary>
    public class RawKnxDeviceMetadata
    {

        string _Identifier;
        public string Identifier
        {
            get => _Identifier;
        }

        int _ObjectId;
        public int ObjectId
        {
            get => _ObjectId;
        }

        int _ValueId;
        public int ValueId
        {
            get => _ValueId;
        }
        private Type _ValueDataType;
        public Type ValueDataType
        {
            get => _ValueDataType;
        }

        private Byte[] _RawPropertyValue;
        public Byte[] RawPropertyValue
        {
            get => _RawPropertyValue;
            internal set => _RawPropertyValue = value;
        }

        private object? _PropertyValue;
        public object? PropertyValue
        {
            get => getPropertyValueObject();
        }

        private bool _TriedReading;
        public bool TriedReading
        {
            get => _TriedReading;
            internal set => _TriedReading = value;
        }

        private bool _ReadSuccessful;
        public bool ReadSuccessful
        {
            get => _ReadSuccessful;
            internal set => _ReadSuccessful = value;
        }

 

        public RawKnxDeviceMetadata(string pIdentifier, Type pValueDataType, int pObjectId, int pValueId)
        {
            // Check for unsupported Types here

            _Identifier = pIdentifier;
            _ObjectId = pObjectId;
            _ValueId = pValueId;
            _ValueDataType = pValueDataType;

        }

        private object? getPropertyValueObject()
        {
            if (!this.ReadSuccessful || this.RawPropertyValue.Length == 0)
                return null;

            if (ValueDataType == typeof(Int32))
            {
                return BitConverter.ToInt32(this.RawPropertyValue.Reverse().ToArray(),0);
            }
            else if (ValueDataType == typeof(UInt32))
            {
                return BitConverter.ToUInt32(this.RawPropertyValue.Reverse().ToArray(),0);
            }

            else if (ValueDataType == typeof(Int16))
            {
                return BitConverter.ToInt16(this.RawPropertyValue.Reverse().ToArray(),0);
            }
            else if (ValueDataType == typeof(UInt16))
            {
                return BitConverter.ToUInt16(this.RawPropertyValue.Reverse().ToArray(),0);
            }

            else if (ValueDataType == typeof(string))
            {
                return ASCIIEncoding.ASCII.GetString(this.RawPropertyValue);

            }
            else if (ValueDataType == typeof(object))
            {
                return this.RawPropertyValue;
            }


            else
            {
                throw new InvalidOperationException("Target type " + ValueDataType.ToString() + " is not supported (yet)");
            }
                

        }
    }


}

