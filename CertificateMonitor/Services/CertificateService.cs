using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CertificateMonitor.Models;

namespace CertificateMonitor.Services
{
    public class CertificateService
    {
        private readonly DatabaseService _dbService;
        private readonly ConcurrentDictionary<string, DateTime> _lastLogged = new ConcurrentDictionary<string, DateTime>();
        private readonly TimeSpan _deduplicationWindow = TimeSpan.FromSeconds(60);

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
            return GetWindowText(handle, buff, nChars) > 0 ? buff.ToString() : "Unknown";
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

        private string GetUrlFromWindowTitle(string processName, string windowTitle)
        {
            string[] browserProcesses = { "chrome", "firefox", "msedge", "iexplore" };
            if (browserProcesses.Contains(processName, StringComparer.OrdinalIgnoreCase))
            {
                return windowTitle.Contains(" - ") ? windowTitle.Split(" - ")[0] : "Unknown";
            }
            return null;
        }

        public void MonitorCertificateUsage()
        {
            try
            {
                using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

                string currentProcess = GetActiveProcessName();
                string currentWindow = GetActiveWindowTitle();
                string url = GetUrlFromWindowTitle(currentProcess, currentWindow);

                foreach (var cert in store.Certificates)
                {
                    if (cert.HasPrivateKey)
                    {
                        try
                        {
                            using (var csp = cert.GetRSAPrivateKey())
                            {
                                if (csp != null)
                                {
                                    // Check for duplicates
                                    string key = $"{cert.Thumbprint}_{currentProcess}";
                                    if (_lastLogged.TryGetValue(key, out var lastLoggedTime) &&
                                        DateTime.Now - lastLoggedTime < _deduplicationWindow)
                                    {
                                        continue; // Skip duplicate
                                    }

                                    // Perform a lightweight operation to check key accessibility
                                    byte[] dummyData = Encoding.UTF8.GetBytes("test");
                                    byte[] signature = csp.SignData(dummyData, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                                    // Log the certificate usage
                                    var log = new CertificateLog
                                    {
                                        Timestamp = DateTime.Now,
                                        ProcessName = currentProcess,
                                        WindowTitle = currentWindow,
                                        Url = url,
                                        CertificateSubject = cert.Subject,
                                        Thumbprint = cert.Thumbprint
                                    };

                                    _dbService.SaveCertificateLog(log);
                                    _lastLogged[key] = DateTime.Now;
                                    Console.WriteLine($"Certificate used: {cert.Subject}, Thumbprint: {cert.Thumbprint}, Process: {currentProcess}, Window: {currentWindow}, URL: {url ?? "N/A"}");
                                }
                            }
                        }
                        catch (CryptographicException ex)
                        {
                            Console.WriteLine($"Error accessing private key for {cert.Subject}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error monitoring certificates: {ex.Message}");
            }
        }
    }
}