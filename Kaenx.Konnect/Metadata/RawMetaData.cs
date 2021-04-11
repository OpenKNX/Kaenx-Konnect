using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Metadata
{

    public class RawMetaData
    {
        /// <summary>
        /// Defines the possible read lists. Empty just provides an empty list for you to fill, Generic will read all Properties from 0 to 128 and Default some commonly used parameters
        /// </summary>
        public enum ReadListDefinition { Empty, Generic, Default}

        private Dictionary<string, RawKnxDeviceMetadata> _readList;


        /// <summary>
        /// Contains the properties to read
        /// </summary>
        public Dictionary<string, RawKnxDeviceMetadata> ReadList
        {
            get => _readList;
            internal set => _readList = value;
        }

        // Initalizes the Raw Meta Data Object, usually with a default property list
        public RawMetaData(ReadListDefinition pDef)
        {
            _readList = new Dictionary<string, RawKnxDeviceMetadata>();

            if (pDef == ReadListDefinition.Default)
            {
                foreach (RawKnxDeviceMetadata k in GenerateDefaultPropertyList())
                    _readList.Add(k.Identifier, k);
            }
            else if (pDef == ReadListDefinition.Generic)
            {
                foreach (RawKnxDeviceMetadata k in GenerateGenericList())
                    _readList.Add(k.Identifier, k);
            }
            else if (pDef == ReadListDefinition.Empty)
            {
        
            }
        }

        public List<RawKnxDeviceMetadata> GenerateGenericList()
        {
            List<RawKnxDeviceMetadata> l = new List<RawKnxDeviceMetadata>();

            for (int i = 0; i < 129; i++)
            {
                l.Add(new RawKnxDeviceMetadata("GENERIC_" + i.ToString(), typeof(object), 1, i));

            }
            return l;
        }


        public List<RawKnxDeviceMetadata> GenerateDefaultPropertyList()
        {
            List<RawKnxDeviceMetadata> l = new List<RawKnxDeviceMetadata>();
           l.Add(new RawKnxDeviceMetadata("ObjectName", typeof(string), 0, 2));
           l.Add(new RawKnxDeviceMetadata("SerialNumber", typeof(string), 0, 11));
           l.Add(new RawKnxDeviceMetadata("ManufacturerId", typeof(UInt16), 0, 12));
           l.Add(new RawKnxDeviceMetadata("PEIType", typeof(string), 0, 16));
           l.Add(new RawKnxDeviceMetadata("ProductId" , typeof(string), 0, 55));
           l.Add(new RawKnxDeviceMetadata("ManufacturerData", typeof(string), 0,19));
            l.Add(new RawKnxDeviceMetadata("Description", typeof(string), 0, 21));

            return l;
        }


        public void RegisterProperty(RawKnxDeviceMetadata pKvp)
        {
            _readList.Add(pKvp.Identifier, pKvp);
        }

        public void RegisterProperty(string pIdentifier, Type pValueDataType, int pObjectId, int pValueId)
        {
            _readList.Add(pIdentifier, new RawKnxDeviceMetadata(pIdentifier, pValueDataType, pObjectId, pValueId));
        }


    }
}
