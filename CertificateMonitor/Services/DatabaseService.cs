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

            // Check for recent duplicates (within 60 seconds)
            string checkQuery = "SELECT COUNT(*) FROM certificate_logs " +
                               "WHERE thumbprint = @thumbprint AND process_name = @processName " +
                               "AND timestamp > @minTimestamp";
            using var checkCommand = new MySqlCommand(checkQuery, connection);
            checkCommand.Parameters.AddWithValue("@thumbprint", log.Thumbprint);
            checkCommand.Parameters.AddWithValue("@processName", log.ProcessName);
            checkCommand.Parameters.AddWithValue("@minTimestamp", DateTime.Now.AddSeconds(-60));
            long count = (long)checkCommand.ExecuteScalar();

            if (count > 0)
            {
                Console.WriteLine($"Skipping duplicate log for thumbprint: {log.Thumbprint}, process: {log.ProcessName}");
                return; // Skip duplicate
            }

            string insertQuery = "INSERT INTO certificate_logs (timestamp, process_name, window_title, url, certificate_subject, thumbprint) " +
                                "VALUES (@timestamp, @processName, @windowTitle, @url, @subject, @thumbprint)";
            using var insertCommand = new MySqlCommand(insertQuery, connection);
            insertCommand.Parameters.AddWithValue("@timestamp", log.Timestamp);
            insertCommand.Parameters.AddWithValue("@processName", log.ProcessName);
            insertCommand.Parameters.AddWithValue("@windowTitle", log.WindowTitle);
            insertCommand.Parameters.AddWithValue("@url", log.Url ?? (object)DBNull.Value);
            insertCommand.Parameters.AddWithValue("@subject", log.CertificateSubject);
            insertCommand.Parameters.AddWithValue("@thumbprint", log.Thumbprint);

            insertCommand.ExecuteNonQuery();
        }
    }
}