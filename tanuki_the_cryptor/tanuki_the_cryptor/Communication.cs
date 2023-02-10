using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace tanuki_the_cryptor
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
            catch { }
        }

        public void SendMessage(byte[] payload)
        {
            try
            {
                stream.Write(payload, 0, payload.Length);
            }
            catch (Exception ex) { }
            
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

        public void Close()
        {
            stream.Close();
            client.Close();
        }
    }
}
