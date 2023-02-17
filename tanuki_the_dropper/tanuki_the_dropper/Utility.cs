using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tanuki_the_dropper
{
    static public class Utility
    {
        public static void ConsoleLog(string message)
        {
            bool print = true;

            if (print == true)
                Console.WriteLine(message);
        }
    }
}
