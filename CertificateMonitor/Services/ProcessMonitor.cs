using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace CertificateMonitor.Services
{
    public class ProcessMonitor
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        public static (string ProcessName, string WindowTitle) GetActiveProcessInfo()
        {
            const int nChars = 256;
            StringBuilder buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, buff, nChars) > 0)
            {
                try
                {
                    var process = Process.GetProcesses()
                        .FirstOrDefault(p => p.MainWindowHandle == handle);
                    return (process?.ProcessName ?? "Unknown", buff.ToString());
                }
                catch
                {
                    return ("Unknown", buff.ToString());
                }
            }
            return ("Unknown", "Unknown");
        }
    }
}