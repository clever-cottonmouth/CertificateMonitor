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
        private readonly ConcurrentDictionary<string, bool> _inaccessibleCertificates = new ConcurrentDictionary<string, bool>();
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
            StringBuilder buff = new StringBuilder(nChars); // Fixed typo: buffokr -> buff
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
            // Extended list of browser processes
            string[] browserProcesses = { "chrome", "firefox", "msedge", "iexplore", "opera", "safari" };
            if (browserProcesses.Contains(processName, StringComparer.OrdinalIgnoreCase))
            {
                return windowTitle.Contains(" - ") ? windowTitle.Split(" - ")[0] : "Unknown";
            }
            return null; // Non-browser apps may not have URLs
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
                    // Skip certificates without private keys or known to be inaccessible
                    if (!cert.HasPrivateKey || _inaccessibleCertificates.ContainsKey(cert.Thumbprint))
                    {
                        continue;
                    }

                    try
                    {
                        using (var csp = cert.GetRSAPrivateKey())
                        {
                            if (csp == null)
                            {
                                Console.WriteLine($"No accessible private key for {cert.Subject} (Thumbprint: {cert.Thumbprint})");
                                _inaccessibleCertificates.TryAdd(cert.Thumbprint, true);
                                continue;
                            }

                            // Check for duplicates
                            string key = $"{cert.Thumbprint}_{currentProcess}";
                            if (_lastLogged.TryGetValue(key, out var lastLoggedTime) &&
                                DateTime.Now - lastLoggedTime < _deduplicationWindow)
                            {
                                continue;
                            }

                            // Perform a lightweight operation to verify private key accessibility
                            try
                            {
                                byte[] dummyData = Encoding.UTF8.GetBytes("test");
                                byte[] signature = csp.SignData(dummyData, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                                // Log the certificate usage
                                var log = new CertificateLog
                                {
                                    Timestamp = DateTime.Now,
                                    ProcessName = currentProcess,
                                    WindowTitle = currentWindow,
                                    Url = url, // May be null for non-browser apps
                                    CertificateSubject = cert.Subject,
                                    Thumbprint = cert.Thumbprint
                                };

                                _dbService.SaveCertificateLog(log);
                                _lastLogged[key] = DateTime.Now;
                                Console.WriteLine($"Certificate used: {cert.Subject}, Thumbprint: {cert.Thumbprint}, Process: {currentProcess}, Window: {currentWindow}, URL: {url ?? "N/A"}");
                            }
                            catch (CryptographicException ex)
                            {
                                Console.WriteLine($"Failed to sign data with private");

                                _inaccessibleCertificates.TryAdd(cert.Thumbprint, true);
                            }
                        }
                    }
                    catch (CryptographicException ex)
                    {
                        Console.WriteLine($"Error accessing private key for {cert.Subject} (Thumbprint: {cert.Thumbprint}): {ex.Message}");
                        _inaccessibleCertificates.TryAdd(cert.Thumbprint, true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Unexpected error for {cert.Subject} (Thumbprint: {cert.Thumbprint}): {ex.Message}");
                        _inaccessibleCertificates.TryAdd(cert.Thumbprint, true);
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