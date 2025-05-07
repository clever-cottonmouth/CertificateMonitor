using System.Security.Cryptography.X509Certificates;

namespace CertificateMonitor.Helpers
{
    public static class CertificateHelper
    {
        public static X509Certificate2Collection GetCertificates(StoreName storeName, StoreLocation storeLocation)
        {
            using (var store = new X509Store(storeName, storeLocation))
            {
                store.Open(OpenFlags.ReadOnly);
                return store.Certificates;
            }
        }

        public static X509Certificate2 FindCertificateByThumbprint(string thumbprint)
        {
            var stores = new[] { StoreName.My, StoreName.Root, StoreName.TrustedPeople, StoreName.CertificateAuthority };
            foreach (var storeName in stores)
            {
                var certificates = GetCertificates(storeName, StoreLocation.CurrentUser);
                var cert = certificates.Find(X509FindType.FindByThumbprint, thumbprint, false)
                    .OfType<X509Certificate2>().FirstOrDefault();
                if (cert != null)
                {
                    Console.WriteLine($"Certificate found in {storeName} (CurrentUser): {thumbprint}");
                    return cert;
                }

                certificates = GetCertificates(storeName, StoreLocation.LocalMachine);
                cert = certificates.Find(X509FindType.FindByThumbprint, thumbprint, false)
                    .OfType<X509Certificate2>().FirstOrDefault();
                if (cert != null)
                {
                    Console.WriteLine($"Certificate found in {storeName} (LocalMachine): {thumbprint}");
                    return cert;
                }
            }
            Console.WriteLine($"Certificate not found: {thumbprint}");
            return null;
        }
    }
}