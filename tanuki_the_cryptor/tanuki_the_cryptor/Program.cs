using System.Security.Cryptography;
using System.Text;

namespace tanuki_the_cryptor
{
    class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            
            Malware malware = new Malware();
            malware.Run();
            
        } 
    }
}