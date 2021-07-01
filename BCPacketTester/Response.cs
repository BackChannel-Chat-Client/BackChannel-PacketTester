using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCPacketTester
{
    public class Response
    {
        public UInt32 PacketSize { get; set; }
        public UInt32 PacketID { get; set; }
        public string ResponseStatus { get; set; }
        public string ResponseBody { get; set; }
        public string ResponseBodyHex { get; set; }
    }
}
