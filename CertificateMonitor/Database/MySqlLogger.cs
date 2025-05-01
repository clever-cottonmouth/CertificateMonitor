using MySql.Data.MySqlClient;
//using CertificateMonitor.Models;
using CertificateMonitor.Model;

namespace CertificateMonitor.Database
{
    public class MySqlLogger
    {
        private readonly string _connectionString;

        public MySqlLogger(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void LogCertificateUsage(CertificateUsage usage)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    Console.WriteLine("MySQL connection successful");
                    var query = @"INSERT INTO CertificateUsage (ProcessName, WindowName, Url, CertificateUsed, Thumbprint, DateTime)
                                  VALUES (@ProcessName, @WindowName, @Url, @CertificateUsed, @Thumbprint, @DateTime)";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProcessName", usage.ProcessName);
                        command.Parameters.AddWithValue("@WindowName", usage.WindowName);
                        command.Parameters.AddWithValue("@Url", usage.Url);
                        command.Parameters.AddWithValue("@CertificateUsed", usage.CertificateUsed);
                        command.Parameters.AddWithValue("@Thumbprint", usage.Thumbprint);
                        command.Parameters.AddWithValue("@DateTime", usage.DateTime);
                        command.ExecuteNonQuery();
                    }
                }

                File.AppendAllText("D:\\Logs\\CertificateMonitor.log",
           $"[{DateTime.Now}] Logged: {usage.ProcessName}, {usage.Thumbprint}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MySQL Error: {ex.Message}");
                File.AppendAllText("D:\\Logs\\CertificateMonitor_error.log",
            $"[{DateTime.Now}] Error: {ex.Message}\n");
            }
        }
    }
}