using Kaenx.Konnect.EMI;
using Kaenx.Konnect.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaenx.Konnect.Telegram.Contents
{
    public class EmiContent : IContent
    {

        public IEmiMessage Message { get; private set; }
        public ExternalMessageInterfaces Emi {  get; private set; }

        public EmiContent(IEmiMessage message, ExternalMessageInterfaces emi)
        {
            Message = message;
            Emi = emi;
        }

        public EmiContent(byte[] data, ExternalMessageInterfaces emi)
        {
            Emi = emi;

            MessageCodes messageCode = (MessageCodes)data[0];

            if(messageCode == MessageCodes.L_Data_req ||
               messageCode == MessageCodes.L_Data_ind ||
               messageCode == MessageCodes.L_Data_con)
            {
                Message = new EMI.LData.LDataBase(data, emi);
            }
            else
            {
                throw new NotImplementedException("EMI MessageCode not implemented: " + messageCode.ToString());
            }
        }

        public int Length { get; private set; } = 0;

        public byte[] ToByteArray()
        {
            byte[] data = Array.Empty<byte>();

            switch(Emi)
            {
                case ExternalMessageInterfaces.cEmi:
                    data = Message.GetBytesCemi();
                    break;
                case ExternalMessageInterfaces.Emi1:
                    data = Message.GetBytesEmi1();
                    break;
                case ExternalMessageInterfaces.Emi2:
                    data = Message.GetBytesEmi2();
                    break;
                default:
                    throw new NotImplementedException("Unknown ExternalMessageInterface: " + Emi.ToString());
            }

            Length = data.Length;
            return data;
        }
    }
}
