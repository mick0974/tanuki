using System;
using System.Numerics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace tanuki_the_first
{
    public static class Cryptography
    {
        static public (BigInteger, BigInteger, BigInteger) GenerateDHValues_X_GX_KEY(BigInteger prime, BigInteger generator, BigInteger gx_server)
        {
            Random rand = new Random();
            BigInteger x;

            do
            {
                byte[] x_bytes = new byte[128];
                rand.NextBytes(x_bytes);
                x_bytes[x_bytes.Length - 1] &= (byte)0x7F; //force sign bit to positive
                x = new BigInteger(x_bytes);
                Console.WriteLine("x computed: " + x);
            } while (x.IsOne || x.IsZero || BigInteger.Compare(x, BigInteger.Subtract(prime, BigInteger.One)) > 0);
            Console.WriteLine("x computed: " + x);

            BigInteger gx = BigInteger.ModPow(generator, x, prime);
            BigInteger key = BigInteger.ModPow(gx_server, x, prime);

            return (x, gx, key);   
        }
        static public byte[] ComputeAESKey(byte[] exchanged_key)
        {
            string salt = "000390fcbf6de49e9ae24b040c466156";
            byte[] saltBytes = StringToByteArray(salt);
            byte[] derived;

            try
            {
                using (var pbkdf2 = new Rfc2898DeriveBytes(
                    exchanged_key,
                    saltBytes,
                    480000,
                    HashAlgorithmName.SHA256))
                {
                    derived = pbkdf2.GetBytes(32);
                }

                return derived;
            }
            catch(Exception ex)
            {
                Console.WriteLine("compute aes: " + ex.Message);
                return null;
            }
        }

        private static byte[] StringToByteArray(String hex)
        {
            byte[] bytes;
            try
            {
                int NumberChars = hex.Length;
                bytes = new byte[NumberChars / 2];
                for (int i = 0; i < NumberChars; i += 2)
                {
                    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                    Console.WriteLine($"Byte written: {bytes[i / 2]}");
                }
                
                return bytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine("StringToByteArray error: " + ex.Message);
                return null;
            }
        }

        static public byte[] AESDecrypt(byte[] content, byte[] key)
        {
            byte[] iv = StringToByteArray("39978f62a082e13fad6289112cccff09");
            byte[] plaintext = null;

            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.IV = iv;

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    using (MemoryStream msDecrypt = new MemoryStream(content))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                csDecrypt.CopyTo(ms);
                                plaintext = ms.ToArray();
                            }
                        }
                    }
                }

                return plaintext;
            }
            catch (Exception e) { Console.WriteLine("AES descryption error: " + e.Message); return null; }
        }
    }
}
