using System.Diagnostics;

namespace Xperience.Xman.Services
{
    public class ShellRunner : IShellRunner
    {
        public Process Execute(ShellOptions options)
        {
            Process cmd = new();
            cmd.StartInfo.FileName = "powershell.exe";
            cmd.StartInfo.Arguments = "-noprofile -nologo";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.CreateNoWindow = true;
            if (!string.IsNullOrEmpty(options.WorkingDirectory))
            {
                cmd.StartInfo.WorkingDirectory = options.WorkingDirectory;
            }

            if (options.ErrorHandler is not null)
            {
                cmd.StartInfo.RedirectStandardError = true;
                cmd.EnableRaisingEvents = true;
                cmd.ErrorDataReceived += options.ErrorHandler;
            }

            if (options.OutputHandler is not null)
            {
                cmd.StartInfo.RedirectStandardOutput = true;
                cmd.OutputDataReceived += options.OutputHandler;
            }

            cmd.Start();

            if (options.ErrorHandler is not null)
            {
                cmd.BeginErrorReadLine();
            }

            if (options.OutputHandler is not null)
            {
                cmd.BeginOutputReadLine();
            }

            cmd.StandardInput.AutoFlush = true;
            cmd.StandardInput.WriteLine(options.Script);
            if (!options.KeepOpen)
            {
                cmd.StandardInput.Close();
            }

            return cmd;
        }
    }
}
