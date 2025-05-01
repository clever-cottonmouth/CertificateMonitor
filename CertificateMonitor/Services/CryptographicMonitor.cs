using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;
using System;

namespace CertificateMonitor.Services
{
    public class CryptographicMonitor
    {
        public static void StartMonitoring(Action<string, string> onCertificateUsed)
        {
            try
            {
                using (var session = new TraceEventSession("CertificateMonitorSession"))
                {
                    // Enable the Microsoft-Windows-CAPI2 provider
                    session.EnableProvider("Microsoft-Windows-CAPI2", TraceEventLevel.Verbose);

                    // Handle dynamic events
                    session.Source.Dynamic.All += (TraceEvent data) =>
                    {
                        // Filter for certificate-related events (simplified)
                        if (data.ProviderName == "Microsoft-Windows-CAPI2" &&
                            data.EventName.Contains("CertGetCertificateContextProperty") ||
                            data.EventName.Contains("CryptAcquireCertificatePrivateKey"))
                        {
                            try
                            {
                                // Extract thumbprint and process name (simplified)
                                string thumbprint = data.PayloadByName("Thumbprint")?.ToString() ?? "Unknown";
                                string processName = data.ProcessName ?? "Unknown";

                                if (!string.IsNullOrEmpty(thumbprint) && thumbprint != "Unknown")
                                {
                                    onCertificateUsed?.Invoke(thumbprint, processName);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"ETW Event Error: {ex.Message}");
                            }
                        }
                    };

                    // Process events (runs until session is disposed)
                    session.Source.Process();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ETW Session Error: {ex.Message}");
            }
        }
    }
}