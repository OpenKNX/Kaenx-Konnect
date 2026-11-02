using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Telegram
{
    public interface IContent
    {
        public int Length { get; }

        public byte[] ToByteArray();
    }
}
