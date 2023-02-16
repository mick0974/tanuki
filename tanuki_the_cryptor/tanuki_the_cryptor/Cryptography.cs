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
            string hex_private_key_recvKey = "308204A50201000282010100BF27AA20A43C82DF27DAC40875080D7D496A1D1038A89D1EEAE1BA4013694E81E91E82056FA3F069FC2CC4AB6AFCC2175F4E3A4984DCFB1C4831556848A4A03AE471252AB94B16E17CB10FEE2A7617010051790E56298272CF89E289E403CCA01105C661EACB824B9524647BDD0BA42CE283012D2B858438D24520A5282EE5F30C9F655618337A06B5AE07759EE8D851C948AE0051549C1B888A13D9384AB8B1951C57ACF49D342AE9880CAC553BE43A75616BC90728B8F5A70CC61E6D0EFA9FD017E2221AB60392FD99FFDE4701128C2E743D2EFEDA645063AB0356FC89E90831DF95C5D4B286F8F03674CD9D5146DBCE1A3EB6EAF78C556177D5CBB1C64CF50203010001028201000E381A1CA17AEAD544B6AE3C5ED041889F4C686B0B7BE76C2456B42F0E0298576B8ADC6B2119D5C95C7BCC096044FAFF1814CE9B2769F4B7EEDAD49A9444C2B27EC9B7D50CC17C2B3BC64404185E7E6991A77DD4C62E02491E0D08AFEF29253086F3A205289D08CD2C83EF819338C9769946DB708BBB3BD1AA3A802A28EA0C2ABE42B588C23B10050546CB12E17332CCB9FC5F090CD4494C1DD8FCE814BDA18141CE87070BFA8F063972EB5D52A95357B066A6400F6E2C756B87634AD881E2E5DB470F629E11185CA471D8A1A81F21CDFA9073AE4A3C79F2FB9E37C98C780BF5D87DF6B6894DD20E5B6435EA5F6C92487C1C7752B2EFA386C589AB6BC870828502818100D24E3779717BB057F06B3E11F9D487F9C53EB7F75C9BF0DBC1BE8D670E455454F571BFB584036A519D10BF440A1BFAFA52C33B31FF626B4EDDBE3858FB54B9199DFC1D5C95A5DC0AC62EEA21487238D48178963907E3FC5042DBF6793DA643CC8BE478914F4C8E0D9FA14F97403D3B2C8306341A04E5DEFF7F940FC08319A11702818100E8B03CF606FBB9ECAAE8C2E50FBA8E6AFBF261B7769161F8FB4D66FCB623F10CAECD27DFC198A20E34EEC761C7CEFD197FCF7713D14E7992D506FCFB2CE5B71D292F87FA88B39AB4DCCEDAF333583040B6FD879FCBA6D6EF9BFE4A5EF75C9CC8B9FF912090B8EC76DECF5DAF7E2A105AFD4F335AAA9B576727DDBC9EC02C11D302818100C11A30B5C20DE08DDEA39A0AC76AEEAFD8FD0DCE83AA6C2E5C67AB4EC53BC3837F1B42FC588B0A448603AA9BBEEC9236E7677C231C6C323BE83F915DA2E8D84D3D531162C1C5D995CB03A8D786BDDA90C59103DEB9F00CED6576B389FFA17AFF8633F2C0FA1F41102152ECD1E49A548B3A83A7B37C6BD5A6A46E3F487518436102818100B17B5BC43CE75C954C7765D0DA026E06E44DA8830B8930B57CD93928A0B521738F1124CCE319CCE21135E01691152CB07A70805F3953261FFD24EB699A814CFE8D1F98145C98C1F41A481D714B4484E997AA21FC9C9591740A04182DFF77408F4A6FC8FE91E4BB589FF2837F0C38816925B577723C97683F62851B0DEB7EB1C90281810091D86747447055824C2D0BB095B2092A801FEC5E632D21E07F85771643E82D1FE1AA64655E30AD4C60312AE8836F2574AC82FBA7C341CF67C7F17A8DA93A8C8AC431FDEFF06C5DEB71DCFABAC479202D5A86BC791D7066013B8BBA1C014DDCFD6977D32139B384C3E32888EFE950BECE609AE998CE3C9C7D6D6B0F139E1DAC38";

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

        static public byte[] AESEncrypt(byte[] content, byte[] key, byte[] iv)
        {
            byte[] encrypted = null;

            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.IV = iv;
                    aesAlg.Padding = PaddingMode.PKCS7;

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.BaseStream.Write(content, 0, content.Length);
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
            catch (Exception e) { throw; }
        }

        public static byte[] GetHash(byte[] content)
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                try
                {
                    return mySHA256.ComputeHash(content);
                }
                catch (IOException e)
                {
                    throw;
                }
            }
        }
    }
}
