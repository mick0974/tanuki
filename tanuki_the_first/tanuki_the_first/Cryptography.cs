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
        static public byte[] ComputeAESKey(string exchanged_key)
        {
            string salt = "000390fcbf6de49e9ae24b040c466156";
            byte[] saltBytes = StringToByteArray(salt);
            byte[] derived;

            if (exchanged_key.Length % 2 != 0)
                exchanged_key = exchanged_key + "0";
            byte[] key = StringToByteArray(exchanged_key);

            try
            {
                using (var pbkdf2 = new Rfc2898DeriveBytes(
                    key,
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
            Console.WriteLine("LEN: " + "0AA23980CEA62C081B46EB02F7B1B8EA423F3A5995A413D5B4C59516BE1916DAF51D022153388E87785B7B7E5178DDF1ABF0020D6FFFA0B57CF245FE585CE20D3F3EAB8BD555EEB2E5CD076747415EC876061D3500365C5CDF379AB7280B5E1850492FC45E91DFA0ED9610719F6D21E38B8221E6CA9617D6051EF61A9B615BA1E".Length);
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
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.None;

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
