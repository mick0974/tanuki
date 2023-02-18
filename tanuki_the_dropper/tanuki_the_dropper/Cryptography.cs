using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace tanuki_the_dropper
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
            } while (x.IsOne || x.IsZero || BigInteger.Compare(x, BigInteger.Subtract(prime, BigInteger.One)) > 0);

            BigInteger gx = BigInteger.ModPow(generator, x, prime);
            BigInteger key = BigInteger.ModPow(gx_server, x, prime);

            return (x, gx, key);
        }
        static public byte[] ComputeAESKey(string exchanged_key)
        {
            string salt = "000390fcbf6de49e9ae24b040c466156";
            byte[] saltBytes = StringToByteArray(salt);
            byte[] derived;

            exchanged_key = exchanged_key.TrimStart('0');
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
            catch (Exception ex)
            {
                Utility.ConsoleLog($"Error generating AES key: {ex.Message}");
                throw;
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
                    aesAlg.Padding = PaddingMode.PKCS7;

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
            catch (Exception e) { Utility.ConsoleLog($"AES descryption error: {e.Message}"); throw; }
        }

        public static string GetHash(byte[] content)
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                try
                {
                    byte[] hashValue = mySHA256.ComputeHash(content);

                    StringBuilder sb = new StringBuilder();
                    foreach (Byte b in hashValue)
                        sb.Append(b.ToString("x2"));
                    return sb.ToString();
                }
                catch (IOException e)
                {
                    Utility.ConsoleLog($"Sha256 Error: {e.Message}");
                    throw;
                }
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
                }

                return bytes;
            }
            catch (Exception ex)
            {
                Utility.ConsoleLog($"StringToByteArray error: {ex.Message}");
                throw;
            }
        }
    }

}
