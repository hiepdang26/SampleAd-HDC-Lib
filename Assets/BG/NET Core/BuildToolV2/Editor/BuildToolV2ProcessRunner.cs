using System.Diagnostics;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2ProcessRunner
    {
        public static bool TryRunAdbShell(string adbPath, string serial, string shellCommand, out string output)
        {
            output = string.Empty;
            if (string.IsNullOrWhiteSpace(serial) || string.IsNullOrWhiteSpace(shellCommand))
                return false;

            return TryRunProcess(adbPath, $"-s {serial} shell {shellCommand}", out output);
        }

        public static bool TryRunProcess(string fileName, string arguments, out string output)
        {
            output = string.Empty;

            try
            {
                using var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                process.Start();
                string stdOut = process.StandardOutput.ReadToEnd();
                string stdErr = process.StandardError.ReadToEnd();
                process.WaitForExit(5000);

                output = string.IsNullOrWhiteSpace(stdOut) ? stdErr : stdOut;
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
