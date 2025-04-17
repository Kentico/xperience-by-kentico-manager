using System.Diagnostics;

using NUnit.Framework;

namespace Xperience.Manager.Tests
{
    public class TestBase
    {
        [TearDown]
        public void TearDown() => File.Delete(Constants.CONFIG_FILENAME);


        /// <summary>
        /// Gets a <see cref="Process"/> which does nothing.
        /// </summary>
        protected static Process GetDummyProcess()
        {
            Process cmd = new();
            cmd.StartInfo.FileName = "powershell.exe";
            cmd.StartInfo.Arguments = "-noprofile -nologo";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.AutoFlush = true;
            cmd.StandardInput.WriteLine("dotnet --version");
            cmd.StandardInput.Close();

            return cmd;
        }
    }
}
