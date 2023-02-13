using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tanuki_the_first
{
    public static class Exec
    {
        static public void ExecProgram(string fileName)
        {
            var proc = new Process();
            proc.StartInfo.FileName = fileName;
            //proc.StartInfo.Arguments = "-v -s -a";

            proc.Start();
            proc.WaitForExit();
            var exitCode = proc.ExitCode;
            proc.Close();
        }
    }
}
