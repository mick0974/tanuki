using System.Diagnostics;

namespace tanuki_the_dropper
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
