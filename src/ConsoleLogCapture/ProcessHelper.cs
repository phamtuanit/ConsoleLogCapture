using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.Drawing;
using Console = Colorful.Console;

namespace ConsoleLogCapture
{
    /// <summary>
    /// The ProcessHelper class
    /// </summary>
    public class ProcessHelper
    {
        /// <summary>
        /// The logger instance
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ProcessHelper));

        /// <summary>
        /// The process name
        /// </summary>
        private readonly string processName;

        /// <summary>
        /// Gets the exit code.
        /// </summary>
        /// <value>
        /// The exit code.
        /// </value>
        public int ExitCode { get; private set; } = int.MinValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="SMEE.NADAE.Utilities.ProcessHelper" /> class.
        /// </summary>
        /// <param name="processFilePath">The process file path.</param>
        public ProcessHelper(string processFilePath)
        {
            this.processName = processFilePath;
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="waitForExit">if set to <c>true</c> [wait for exit].</param>
        /// <param name="useShellExecute">if set to <c>true</c> [use shell execute].</param>
        /// <exception cref="NADAEException"></exception>
        public void Start(string args, bool waitForExit = true, bool useShellExecute = false)
        {
            using (var process = CreateProcess(this.processName, args, useShellExecute))
            {
                // Execute process
                try
                {
                    var isStreamingData = Logger.IsInfoEnabled && !useShellExecute;
                    if (isStreamingData)
                    {
                        // Register data event to show more information
                        process.OutputDataReceived += this.OnDataReceived;
                        process.ErrorDataReceived += this.OnErrorReceived;
                    }

                    Logger.Info($"[ProcessHelper][Start] Start [{this.processName}] with Args [{args}]...");
                    process.Start();

                    if (isStreamingData)
                    {
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                    }

                    if (waitForExit)
                    {
                        Logger.Debug("[ProcessHelper][Start] Waiting for process completed.");
                        process.WaitForExit();
                        Logger.Info($"[ProcessHelper][Start] Start [{this.processName}] done.");
                    }
                }
                finally
                {
                    if (waitForExit)
                    {
                        this.ExitCode = process.ExitCode;
                    }
                    else
                    {
                        this.ExitCode = 0;
                    }
                    Logger.Info($"[ProcessHelper][Start] The process is exited with Exitcode = [{this.ExitCode}].");

                    if (Logger.IsInfoEnabled && !useShellExecute)
                    {
                        // Register data event to show more information
                        process.OutputDataReceived -= this.OnDataReceived;
                        process.ErrorDataReceived -= this.OnDataReceived;
                    }
                }
            }
        }

        /// <summary>
        /// Cache console output
        /// </summary>
        private void CacheOutput(string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                Logger.Debug(data);
            }
        }

        /// <summary>
        /// Called when [data received].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DataReceivedEventArgs"/> instance containing the event data.</param>
        private void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            CacheOutput(e.Data);

            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                Console.WriteLine(e.Data);
            }
        }

        /// <summary>
        /// Received error
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnErrorReceived(object sender, DataReceivedEventArgs e)
        {
            CacheOutput(e.Data);

            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                Console.WriteLine(e.Data, Color.Red);
            }
        }

        /// <summary>
        /// Creates the command process.
        /// </summary>
        /// <param name="processPath">The process path.</param>
        /// <param name="args">The command string.</param>
        /// <param name="useShellExecute">if set to <c>true</c> [use shell execute].</param>
        /// <returns></returns>
        public static Process CreateProcess(string processPath, string args, bool useShellExecute = false)
        {
            Logger.Info($"Create [{processPath}] process, args = [{args}].");
            var process = new Process();
            var startInfo = new ProcessStartInfo
            {
                FileName = processPath,
                UseShellExecute = useShellExecute,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                RedirectStandardOutput = !useShellExecute,
                RedirectStandardError = !useShellExecute,
                Arguments = args,
                Verb = "runas"
            };

            if (Directory.Exists(processPath))
            {
                startInfo.WorkingDirectory = Path.GetDirectoryName(processPath);
            }

            process.EnableRaisingEvents = true;
            process.StartInfo = startInfo;
            Logger.Info("Create process successfully.");
            return process;
        }

        #region Static methods

        /// <summary>
        /// Executes the specified process.
        /// </summary>
        /// <param name="process">
        /// The process.
        /// </param>
        public static void Execute(Process process)
        {
            Logger.InfoFormat("Start process [{0}]", process.StartInfo.FileName);
            Logger.Debug($"The command detail is: {process.StartInfo.Arguments}");

            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;

            process.Start();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                Logger.Error($"Fail to execute command...");
                Logger.InfoFormat($"Process is failed with Exitcode: {process.ExitCode}");
                throw new Exception("Fail to execute process");
            }
        }

        #endregion
    }
}
