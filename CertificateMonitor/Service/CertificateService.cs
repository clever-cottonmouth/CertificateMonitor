using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CertificateMonitor.Models;

namespace CertificateMonitor.Services
{
    public class CertificateService
    {
        private readonly DatabaseService _dbService;

        public CertificateService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();
            return GetWindowText(handle, buff, nChars) > 0 ? buff.ToString() : null;
        }

        private string GetActiveProcessName()
        {
            IntPtr hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero) return "Unknown";

            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);

            try
            {
                Process p = Process.GetProcessById((int)pid);
                return p.ProcessName;
            }
            catch
            {
                return "Unknown";
            }
        }

        public void CheckCertificates()
        {
            using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            foreach (var cert in store.Certificates)
            {
                var log = new CertificateLog
                {
                    Timestamp = DateTime.Now,
                    ProcessName = GetActiveProcessName(),
                    WindowTitle = GetActiveWindowTitle(),
                    CertificateSubject = cert.Subject,
                    Thumbprint = cert.Thumbprint
                };

                _dbService.SaveCertificateLog(log);
            }
        }
    }
}