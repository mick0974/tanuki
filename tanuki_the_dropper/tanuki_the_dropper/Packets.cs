using System;
using System.Numerics;
using System.Text;
using System.Text.Json;
using tanuki_the_dropper;

namespace Packets
{

    public static class Packet
    {
        public static byte[] GenNewRequest(string operation)
        {
            Packets.Request.NewRequest newRequest = new Packets.Request.NewRequest() { Operation = operation };
            string newRequest_json = JsonSerializer.Serialize<Packets.Request.NewRequest>(newRequest);
            byte[] newRequest_bytes = Encoding.UTF8.GetBytes(newRequest_json.ToString());

            return newRequest_bytes;
        }

        public static (string, string, string) GetServerKeyParameters(byte[] serverHalf_byte)
        {
            string serverHalf = Encoding.UTF8.GetString(serverHalf_byte, 0, serverHalf_byte.Length);
            serverHalf = serverHalf.Replace("'", "\"");
            Packets.Reply.ServerKeyParameters response = JsonSerializer.Deserialize<Packets.Reply.ServerKeyParameters>(serverHalf);
            Utility.ConsoleLog($"Received message: {serverHalf}");

            return (response.Prime, response.Generator, response.Gx_server);
        }

        public static byte[] SendClientParameters(string operation, BigInteger gx)
        {
            Packets.Request.ClientParameters parameters = new Packets.Request.ClientParameters() { Operation = operation, Gx_client = gx.ToString() };
            string parameters_Json = JsonSerializer.Serialize<Packets.Request.ClientParameters>(parameters);
            byte[] parameters_bytes = Encoding.UTF8.GetBytes(parameters_Json.ToString());

            return parameters_bytes;
        }

        public static (string, int) GetExeInfo(byte[] info_bytes)
        {
            string info = Encoding.UTF8.GetString(info_bytes, 0, info_bytes.Length);
            info = info.Replace("'", "\"");
            Packets.Reply.ExeInfo response = JsonSerializer.Deserialize<Packets.Reply.ExeInfo>(info);
            Utility.ConsoleLog($"Received message: {info}");

            return (response.Hash, response.DataLength);
        }
    }

    namespace Request
    {
        public class NewRequest
        {
            public string Operation { get; set; }
        }

        public class ClientParameters
        {
            public string Operation { get; set; }
            public string Gx_client { get; set; }
        }

    }

    namespace Reply
    {
        public class ServerKeyParameters
        {
            public string Operation { get; set; }
            public string Prime { get; set; }
            public string Generator { get; set; }
            public string Gx_server { get; set; }
        }

        public class ExeInfo
        {
            public string Operation { get; set; }
            public string Hash { get; set; }

            public int DataLength { get; set; }
        }
    }
}

