using System.Diagnostics;

namespace Xperience.Manager.Services
{
    public class ShellOptions
    {
        /// <summary>
        /// The script to execute.
        /// </summary>
        public string Script { get; private set; }


        /// <summary>
        /// The absolute path of the script working directory.
        /// </summary>
        public string? WorkingDirectory { get; set; }


        /// <summary>
        /// If <c>true</c>, the <see cref="Process.StandardInput"/> is not closed.
        /// </summary>
        public bool KeepOpen { get; set; }


        /// <summary>
        /// The handler called when the script encounters an error.
        /// </summary>
        public DataReceivedEventHandler? ErrorHandler { get; set; }


        /// <summary>
        /// The handler called when the script outputs data.
        /// </summary>
        public DataReceivedEventHandler? OutputHandler { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="ShellOptions"/> and sets the <see cref="Script"/>.
        /// </summary>
        public ShellOptions(string script) => Script = script;
    }
}
