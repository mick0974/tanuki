using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace tanuki_the_first
{
    public class Communication
    {
        private string server;
        private int port;
        private TcpClient client;
        private NetworkStream stream;

        public Communication(string server, int port)
        {
            this.server = server;
            this.port = port;
        }

        public void Start()
        {
            try
            {
                client = new TcpClient(server, port);
                stream = client.GetStream();
            }
            catch(Exception ex) { Console.WriteLine("Error opening connection: " + ex.Message); }
        }

        public void SendMessage(byte[] payload)
        {
            try
            {
                stream.Write(payload, 0, payload.Length);
            }
            catch (Exception ex) { Console.WriteLine("Error sendig message: " + ex.Message); }

        }

        public (byte[], int) RecvMessage(int byte_to_read)
        {
            try
            {
                byte[] recievedMessage = new byte[byte_to_read];
                int bytesRead = stream.Read(recievedMessage, 0, byte_to_read);

                return (recievedMessage, bytesRead);
            }
            catch (Exception ex) { return (null, 0); }

        }

        public (byte[], int) RecvBinary(int byte_to_read)
        {
            try
            {
                byte[] recievedMessage = new byte[byte_to_read];
                int readSoFar = 0;

                while (readSoFar < byte_to_read)
                {
                    var read = stream.Read(recievedMessage, readSoFar, recievedMessage.Length - readSoFar);
                    readSoFar += read;
                    Console.WriteLine("read now: " + read);
                    Console.WriteLine("readSoFar: " + readSoFar);
                    if (read == 0)
                        break;   // connection was broken
                }


                return (recievedMessage, readSoFar);
            }
            catch (Exception ex) { Console.WriteLine("Error sendig or recieving message: " + ex.Message); return (null, 0); }

        }

        public void Close()
        {
            stream.Close();
            client.Close();
        }
    }
}
