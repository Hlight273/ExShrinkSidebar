using System.Diagnostics;

public static class ScriptRunner
{
    public static void Run(string cmd)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/c " + cmd,
            CreateNoWindow = true
        });
    }
}