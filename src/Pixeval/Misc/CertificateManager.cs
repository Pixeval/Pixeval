using System.Security.Cryptography.X509Certificates;

namespace Pixeval.Misc
{
    public class CertificateManager
    {
        private readonly X509Certificate2 _cert;

        public CertificateManager(X509Certificate2 certificate)
        {
            _cert = certificate;
        }

        public void Install(StoreName storeName, StoreLocation storeLocation)
        {
            using var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadWrite);
            store.Add(_cert);
        }

        public void Uninstall(StoreName storeName, StoreLocation storeLocation)
        {
            using var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadWrite);
            store.Remove(_cert);
        }

        public bool Query(StoreName storeName, StoreLocation storeLocation)
        {
            using var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadOnly);
            return store.Certificates.Find(X509FindType.FindByThumbprint, _cert.Thumbprint, true).Count > 0;
        }
    }
}