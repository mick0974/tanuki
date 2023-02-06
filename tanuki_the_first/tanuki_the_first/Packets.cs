﻿using System;
using System.Numerics;

namespace Packets
{
    namespace Request
    {
        public class KeyExchange
        {
            public string Operation { get; set; }
        }

        public class ParametersExchange
        {
            public string Operation { get; set; }
            public string Gx_client { get; set; }
        }

        public class SymmetricKeyExchange
        {
            public string Operation { get; set; }
        }

        public class ExeExchange
        {
            public string Operation { get; set; }
        }
    }

    namespace Reply
    {
        public class KeyExchange
        {
            public string Operation { get; set; }
            public string Prime { get; set; }
            public string Generator { get; set; }
            public string Gx_server { get; set; }
        }

        public class SymmetricKeyExchange 
        {
            public string Operation { get; set; }
            public string Key { get; set; }
        }

        public class ExeExchange 
        {
            public string Operation { get; set; }
            public string Hash { get; set; }
        }
    }
}
