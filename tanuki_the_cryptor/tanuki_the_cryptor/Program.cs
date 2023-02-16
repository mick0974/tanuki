namespace tanuki_the_cryptor
{
    class Program
    {
        static void Main()
        {
            Utility.ConsoleLog("Start malware.\n");

            Malware malware = new Malware();
            malware.Run();
        }
    }
}