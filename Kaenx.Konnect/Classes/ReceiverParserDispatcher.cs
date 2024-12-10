using Kaenx.Konnect.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Kaenx.Konnect.Classes
{
    class ReceiverParserDispatcher
    {
        private static ReceiverParserDispatcher _instance = new ReceiverParserDispatcher();
        public static ReceiverParserDispatcher Instance
        {
            get {
                return _instance;
            }
        }

        private readonly List<IReceiveParser> _responseParsers;

        public ReceiverParserDispatcher()
        {
            _responseParsers = new List<IReceiveParser>();
            List<Type> parsers = new List<Type>();
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();

            foreach (Type type in types)
            {
                if (type.IsClass && !type.IsNested && type.Namespace == "Kaenx.Konnect.Parser")
                    parsers.Add(type);
            }

            foreach (Type t in parsers)
            {
                object? inst = Activator.CreateInstance(t);
                if(inst == null)
                    throw new Exception("Could not create instance of parser: " + t.FullName);
                IReceiveParser parser = (IReceiveParser)inst;
                _responseParsers.Add(parser);
            }
        }

        public IParserMessage Build(byte[] responseBytes)
        {
            var headerLength = ParseHeaderLength(responseBytes[0]);
            var protocolVersion = ParseProtocolVersion(responseBytes[1]);
            var serviceTypeIdentifier = ParseServiceTypeIdentifier(responseBytes[2], responseBytes[3]);
            var totalLength = ParseTotalLength(responseBytes[4], responseBytes[5]);

            IReceiveParser? parser = _responseParsers.SingleOrDefault(x => x.ServiceTypeIdentifier == serviceTypeIdentifier);
            if(parser == null)
                throw new Exception("No parser found for ServiceTypeIdentifier: " + serviceTypeIdentifier);
            IParserMessage result = parser.Build(headerLength, protocolVersion, totalLength, responseBytes.Skip(6).ToArray());
            return result;
        }

        private static ushort ParseTotalLength(byte data, byte data1)
        {
            return BitConverter.ToUInt16(new[] { data1, data }, 0);
        }

        private static ushort ParseServiceTypeIdentifier(byte data, byte data1)
        {
            return BitConverter.ToUInt16(new[] { data1, data }, 0);
        }

        private static byte ParseProtocolVersion(byte data)
        {
            return data;
        }

        private static byte ParseHeaderLength(byte data)
        {
            return data;
        }
    }
}
