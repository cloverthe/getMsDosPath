

using System.Diagnostics;
using System.Text.RegularExpressions;

namespace getMsDosPath
{
    internal static class Program
    {


        [STAThread]
        static void Main(string[] args)
        {
            // Check if the program is run without any arguments (context menu mode)
            if (args.Length == 1)
            {
      
                string filePath = args[0];

                if (!string.IsNullOrEmpty(filePath))
                {
                    // Convert path to DOS style
                    string dosPath = ConvertToDOSPath(filePath);

                    // Copy DOS path to clipboard
                    CopyTextToClipboard(dosPath);
                }
                return;
            }

            // Handle command-line usage (for testing)
            if (args.Length == 0)
            {
                AddToContextMenu();
            }
        }
        static void CopyTextToClipboard(string text)
        {
            Clipboard.SetText(text);
        }

        static string ConvertToDOSPath(string filePath)
        {
            // Get the drive letter
            string driveLetter = Path.GetPathRoot(filePath).Substring(0, 1);

            // Get the full path without the drive letter
            string fullPathWithoutDrive = filePath.Substring(3);

            // Create a DOS-style 8.3 path
            string dosPath = GetDosShortPath(fullPathWithoutDrive);

            // Combine the drive letter and DOS-style path

            return dosPath;
        }

        static string GetDosShortPath(string filePath)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/c echo {filePath} & for %I in (.) do MSDOSOUTPUTPATH: %~sI";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            string pattern = @"MSDOSOUTPUTPATH:\s*(.+)";
            Match match = Regex.Match(output, pattern);
            if (match.Success)
            {
                string dosOutput = match.Groups[1].Value.Trim();
                return dosOutput;
            }

            return "";
        }



        static void AddToContextMenu()
        {
            string programPath = Process.GetCurrentProcess().MainModule.FileName;
            string registryPath = @"Software\Classes\*\shell\CopyDOSPath";
            using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(registryPath, true))
            {
                if (key == null)
                {
                    using (Microsoft.Win32.RegistryKey subKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(registryPath))
                    {
                        subKey.SetValue("", "Copy DOS Path");
                        subKey.SetValue("Icon", programPath);
                        using (Microsoft.Win32.RegistryKey commandKey = subKey.CreateSubKey("command"))
                        {
                            commandKey.SetValue("", $"\"{programPath}\" \"%1\"");
                        }
                    }
                }
            }
        }
    }
}