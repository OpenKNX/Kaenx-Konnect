using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Telegram.Contents
{
    public class RawContent : IContent
    {
        public int Length { get; }
        private byte[] Data { get; }

        public RawContent(byte[] data)
        {
            Data = data;
            Length = data.Length;
        }

        public byte[] ToByteArray()
        {
            return Data;
        }
    }
}
