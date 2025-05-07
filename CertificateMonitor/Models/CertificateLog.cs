namespace CertificateMonitor.Models
{
    public class CertificateLog
    {
        public DateTime Timestamp { get; set; }
        public string ProcessName { get; set; }
        public string WindowTitle { get; set; }
        public string CertificateSubject { get; set; }
        public string Thumbprint { get; set; }
    }
}