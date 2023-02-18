using System.Net.Sockets;

namespace tanuki_the_dropper
{
    public class Communication
    {
        private string server;
        private int port;
        private TcpClient client = null;
        private NetworkStream stream = null;

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
            catch (Exception ex) { throw; }
        }

        public void SendMessage(byte[] payload)
        {
            try
            {
                stream.Write(payload, 0, payload.Length);
            }
            catch (Exception ex) { throw; }

        }

        public byte[] RecvMessage(int byte_to_read)
        {
            try
            {
                byte[] recievedMessage = new byte[byte_to_read];
                int bytesRead = stream.Read(recievedMessage, 0, byte_to_read);

                return TruncateMessage(recievedMessage, bytesRead);
            }
            catch (Exception ex) { throw; }

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
                    if (read == 0)
                        break;   // connection was broken
                }

                return (recievedMessage, readSoFar);
            }
            catch (Exception ex) { throw; }

        }

        private byte[] TruncateMessage(byte[] fullArray, int bytesRead)
        {
            byte[] truncatedArray = new byte[bytesRead];
            Array.Copy(fullArray, truncatedArray, bytesRead);

            return truncatedArray;
        }

        public void Close()
        {
            if (stream != null) stream.Close();
            if (client != null) client.Close();
        }
    }
}
