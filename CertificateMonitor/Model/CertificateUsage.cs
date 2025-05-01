using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CertificateMonitor.Model
{
    public class CertificateUsage
    {
        public int Id { get; set; }
        public string ProcessName { get; set; }
        public string WindowName { get; set; }
        public string Url { get; set; }
        public string CertificateUsed { get; set; }
        public string Thumbprint { get; set; }
        public DateTime DateTime { get; set; }
    }
}
