using MySql.Data.MySqlClient;
using CertificateMonitor.Models;

namespace CertificateMonitor.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void SaveCertificateLog(CertificateLog log)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string query = "INSERT INTO certificate_logs (timestamp, process_name, window_title, certificate_subject, thumbprint) " +
                           "VALUES (@timestamp, @processName, @windowTitle, @subject, @thumbprint)";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@timestamp", log.Timestamp);
            command.Parameters.AddWithValue("@processName", log.ProcessName);
            command.Parameters.AddWithValue("@windowTitle", log.WindowTitle);
            command.Parameters.AddWithValue("@subject", log.CertificateSubject);
            command.Parameters.AddWithValue("@thumbprint", log.Thumbprint);

            command.ExecuteNonQuery();
        }
    }
}