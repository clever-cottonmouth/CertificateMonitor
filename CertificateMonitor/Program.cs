using CertificateMonitor.Database;
using CertificateMonitor.Helpers;
using CertificateMonitor.Model;
//using CertificateMonitor.Models;
using CertificateMonitor.Services;
using Topshelf;

namespace CertificateMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<CertificateMonitorService>(s =>
                {
                    s.ConstructUsing(name => new CertificateMonitorService());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();
                x.SetDescription("Certificate Usage Monitoring Service");
                x.SetDisplayName("CertificateMonitor");
                x.SetServiceName("CertificateMonitor");
            });
        }
    }

    public class CertificateMonitorService
    {
        private readonly MySqlLogger _logger;
        private bool _running;

        public CertificateMonitorService()
        {
            string connectionString = "Server=localhost;Database=CertificateUsageDB;Uid=root;Pwd=1234;";
            _logger = new MySqlLogger(connectionString);
        }

        public void Start()
        {
            _running = true;
            Task.Run(() =>
            {
                CryptographicMonitor.StartMonitoring((thumbprint, processName) =>
                {
                    if (!_running) return;
                    try
                    {
                        var (activeProcess, windowTitle) = ProcessMonitor.GetActiveProcessInfo();
                        string url = BrowserMonitor.GetBrowserUrl(processName).Result;
                        var certificate = CertificateHelper.FindCertificateByThumbprint(thumbprint);
                        string certName = certificate?.Subject ?? "Unknown";

                        var usage = new CertificateUsage
                        {
                            ProcessName = processName,
                            WindowName = windowTitle,
                            Url = url,
                            CertificateUsed = certName,
                            Thumbprint = thumbprint,
                            DateTime = DateTime.Now
                        };
                        _logger.LogCertificateUsage(usage);

                        Console.WriteLine($"Logged: {processName}, {windowTitle}, {url}, {certName}, {thumbprint}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                });
            });
        }

        public void Stop()
        {
            _running = false;
        }
    }
}