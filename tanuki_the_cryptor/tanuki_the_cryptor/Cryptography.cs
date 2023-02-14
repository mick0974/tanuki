using System.Security.Cryptography;

namespace tanuki_the_cryptor
{
    static public class Cryptography
    {

        public static byte[] RSAEncrypt(byte[] payload)
        {
            string hex_public_key_sendKey = "3082010A0282010100FBEEBDC4608F50E218935DA76A6A12AAC201E522752D011239DE979C4B43E0A1AAB984A64828D72D17923345FE99E77999A6B51433524C74D7B509C3C17F54855411EEBDB45CD1E1DDFC3D60B287C3F67C9EBD5CB9123721435E6DC6B974FB8B1FCF69F7F1C6A6F1424370575383D0BA8DF1EE4DA57609F93E9C373BF5AEAF3B05CB31241D1980A2D14E17C01DE2A7B1A9376DC3A3B4758154DD3092BF4A959F9F9766680F03B02D2EA9479F33E7298BDBF76C225EF1E285DD60D2E9E4194645D9E0C06E03B9E6D5B23B31C2703DFEB778C7FC2353D47372CA963B61CFDDBF557C9949DD109FD6C8D8AD5FC986E47400093430A0139EB16555BB4AFA07D018B50203010001";

            RSA rsa = RSA.Create();
            rsa.ImportRSAPublicKey(Convert.FromHexString(hex_public_key_sendKey), out _);

            try
            {
                byte[] encryptedData;
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.ImportParameters(rsa.ExportParameters(false));
                    encryptedData = RSA.Encrypt(payload, false);
                }
                return encryptedData;
            }
            catch (CryptographicException e) { throw; }
        }

        public static byte[] RSADecrypt(byte[] payload)
        {
            string hex_private_key_recvKey = "308204A50201000282010100FBEEBDC4608F50E218935DA76A6A12AAC201E522752D011239DE979C4B43E0A1AAB984A64828D72D17923345FE99E77999A6B51433524C74D7B509C3C17F54855411EEBDB45CD1E1DDFC3D60B287C3F67C9EBD5CB9123721435E6DC6B974FB8B1FCF69F7F1C6A6F1424370575383D0BA8DF1EE4DA57609F93E9C373BF5AEAF3B05CB31241D1980A2D14E17C01DE2A7B1A9376DC3A3B4758154DD3092BF4A959F9F9766680F03B02D2EA9479F33E7298BDBF76C225EF1E285DD60D2E9E4194645D9E0C06E03B9E6D5B23B31C2703DFEB778C7FC2353D47372CA963B61CFDDBF557C9949DD109FD6C8D8AD5FC986E47400093430A0139EB16555BB4AFA07D018B5020301000102820100400819015D8D6F8057C0B4D00552FA759E23BF2A37DFFD62B584A6C219BB21CFD5459BBA6BA2BDB5FF44B0757CD57F28BFEDC81E5F40EE9FCFE77A98DF884E6228D0F9FE0B66800DC94F0006A0B9B30BE5F6AEA86F7D7AB7A098D28BECBD17E5EF6AD743269757FCFFDB86251BDBE7E4528988090221F33CFF35D330112C99EA8460B9B22F43A71E38F6CCAE35739C6DEBEDAA5291D08F562EED3B30DA48A8555010299A66A272A3E6C025BB2D5CE1D990700ADE6830ABD224A3D1A77DE566C71D42D58F39EB1C8AF7178D3F6EF8B84BD4F20502B40E56E4ADEAB1340CB04A583959A501839FF9FC3549F1BA521DFA228D405EB00303918B3FACDC7D65459BCD02818100FF47DC2E75634D00A10779B4512973FFACAB0766DD55394D0D1C56F27E6FF4645ABF0E6B39C05D9C60F6B10B6FFD5EEFDFF15E46C4A0353C44E6E24DD0FC15DF421EDC2DBA19BC6C52C635770A031FBDA6CFC360A17D5F70C38EAA94AE5695E2AF3DF740C53FE78A1147BB17483E02BBF40894837423CAE70EDD72B4C84151B302818100FCA477536BE5FE3E6D7D69BAED2B848AFD76283CD60FCA7A6B82AA69514A79AC21D3D0A3EB94BA393DA4DF28DB90A9F189EE4DF07520050FECA317E349271B661DA304A9C9B08AB648D763308BE595B42A20C3FDE58C7875775EDFE4C90F2D210B538283535BE9F49AB3AFBA21A10B48FABCAD8CB68A36EFDD770245114827F702818100CB07C11366236B0BB406B901F3870F8DEA2B4040CC89CA80008C688E8686CE0DBEAAB3720E45736E3B24189D2B1D8D93243DD8A85A1BBD1B49058439359C385D0F3E72092038F5C6057344F47F0F0B00901A2F9EBBF175A5BEB2C9F26451D2F5AE408F54814C98FF134D447C78E570D1417E4D77DB37D8593A565FB36E8D9CC7028181008801BE1F51B9DA1A1F756182F904F170A5AC8352E4E65159802132BE49BDEF2C8475741F9AC6514E596A359CD83ED3CE2D33F60F59EE67EB3FB83936E97DBD19472128748318A0442B889064651BAE70F430F971E8AEAB768951B0F4886CBC1DCAEC2519AFB98E8458F81CC4F62FAD54976F157AE816162721315D2E737445D702818100D7BCF41DA06C25BA874376EF388FC01D11BA8065B98C1FD958219E49E8B0411A80DCC3E2B128F12CD3E7236606B50D74A0D176AC6CBCF754CBA54FE96F57523A5B28FF03110C99AACD66E32318A01211E754D103386488B2FBA8BDF3925BA0F9BA35CE9966525273739002A6C0F96A66D4D2C75894BE4359F8183EA0B5EFA05A";

            RSA rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(Convert.FromHexString(hex_private_key_recvKey), out _);

            try
            {
                byte[] encryptedData;
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.ImportParameters(rsa.ExportParameters(true));
                    encryptedData = RSA.Decrypt(payload, false);
                }
                return encryptedData;
            }
            catch (CryptographicException e) { throw; }
        }

        static public byte[] AESEncrypt(string content, byte[] key, byte[] iv)
        {
            byte[] encrypted = null;

            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.IV = iv;

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(content);
                            }
                            encrypted = msEncrypt.ToArray();
                        }
                    }
                }
                return encrypted;
            }
            catch (Exception e) { throw; }
        }

        static public byte[] AESDecrypt(byte[] content, byte[] key, byte[] iv)
        {
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
            catch (Exception e) { throw; }
        }
    }
}
