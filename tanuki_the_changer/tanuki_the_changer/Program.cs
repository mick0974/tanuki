using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace tanuki_the_cypher
{
    class Program
    {
        

        static void Main(string[] args)
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string target = folder + "\\test.txt";

            foreach (var path in Directory.GetFiles(folder))
            {
                Console.WriteLine(path); // full path
                Console.WriteLine(System.IO.Path.GetFileName(path)); // file name
            }

            Console.ReadLine();

            byte[] encrypted = null;
            byte[] key, iv;
            Compare compare = new Compare();

            Console.WriteLine(compare.Equality(File.ReadAllBytes(folder + "\\test.txt"), 
                Encoding.ASCII.GetBytes(File.ReadAllText(folder + "\\test.txt"))));

            using (Aes aesAlg = Aes.Create())
            {
                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                key = aesAlg.Key;
                iv = aesAlg.IV;

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(File.ReadAllText(folder + "\\test.txt"));
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            File.WriteAllBytes(folder + "\\test_encrypted.txt", encrypted);

            //---------------------------------------//

            string plaintext = null;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(File.ReadAllBytes(folder + "\\test_encrypted.txt")))
                {
                    Console.WriteLine(File.ReadAllBytes(folder + "\\test_encrypted.txt").Length);
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
            Console.WriteLine(plaintext);
            File.WriteAllText(folder + "\\test_decrypted.txt", plaintext);
            Console.ReadLine();

        }
    }

    class Compare
    {
        public bool Equality(byte[] a1, byte[] b1)
        {
            int i;
            if (a1.Length == b1.Length)
            {
                i = 0;
                while (i < a1.Length && (a1[i] == b1[i])) //Earlier it was a1[i]!=b1[i]
                {
                    i++;
                }
                if (i == a1.Length)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
