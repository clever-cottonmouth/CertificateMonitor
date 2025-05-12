using CertificateMonitor.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace CertificateMonitor
{
    class Program
    {
        static void Main()
        {
            // Load configuration
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config/appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string connectionString = config.GetConnectionString("MySql");
            var dbService = new DatabaseService(connectionString);
            var certService = new CertificateService(dbService);

            Console.WriteLine("Starting real-time certificate usage monitor with polling for all applications...");

            var timer = new System.Timers.Timer(12000); // 12-second interval
            timer.Elapsed += (s, e) => certService.MonitorCertificateUsage();
            timer.Start();

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
            timer.Stop();
        }
    }
}