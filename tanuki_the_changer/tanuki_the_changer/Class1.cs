using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packets
{
    namespace Request
    {
        public class SendKey
        {
            public string Operation { get; set; }
            public string Key { get; set; }
        }

        public class RetriveKey
        {
            public string Operation { get; set; }
        }
    }

    namespace Reply
    {
        public class RetriveKey
        {
            public string Operation { get; set; }
            public string Key { get; set; }
        }
    }
}
