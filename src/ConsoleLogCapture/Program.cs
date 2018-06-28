using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Colorful;
using Console = Colorful.Console;

namespace ConsoleLogCapture
{
    class Program
    {
        static void Main(string[] args)
        {
            // Register: run process
            ConsoleArgManager.Instance.GetHandlerBuilder()
                .Match("rp", RunProcess)
                .Description("Run process.")
                .Example(@"-rp 'C:\YourApp.exe' '-install serviceName:ServiceA'")
                .Condition(parameters => parameters.Where(i => !string.IsNullOrWhiteSpace(i.Value)).Count() >= 2, "The input parameter count must be more than 2.")
                .WithParam()
                    .Match("pth")
                    .Description("Full path of the process.")
                    .Base()
                .WithParam()
                    .Match("arg")
                    .Description("Arguments for target process.");

            // Register: write logo
            ConsoleArgManager.Instance.GetHandlerBuilder()
                .Match("wrascii", WriteLogo)
                .Description("Write ascii.")
                .Example(@"-wrascii 'Pham Tuan'")
                .Condition(parameters => parameters.Where(i => !string.IsNullOrWhiteSpace(i.Value)).Count() >= 0, "The input parameter count must be more than 0.")
                .WithParam()
                    .Match("txt")
                    .Description("Your text.");

            ConsoleArgManager.Instance.Build(args).Do(args);
        }

        /// <summary>
        /// Writes the logo.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        private static bool WriteLogo(List<Parameter> arg)
        {
            var logoText = arg[0].Value;
            Console.WriteAscii(logoText, Color.Orange);
            return true;
        }

        /// <summary>
        /// Runs the process.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        private static bool RunProcess(List<Parameter> args)
        {
            var processPath = args[0].Value;
            var arg = args[1].Value;

            var process = new ProcessHelper(processPath);
            process.Start(arg);
            return true;
        }
    }
}
