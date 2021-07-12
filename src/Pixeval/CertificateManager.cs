using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Pixeval
{
    public class CertificateManager
    {
        private readonly X509Certificate2 _cert;

        public CertificateManager(X509Certificate2 certificate)
        {
            _cert = certificate;
        }

        public static async Task<X509Certificate2> GetFakeCaRootCertificate()
        {
            return new(await App.GetResourceBytes("ms-appx:///Assets/pixeval_ca.cer"));
        }
        
        public static async Task<X509Certificate2> GetFakeServerCertificate()
        {
            return new(await App.GetResourceBytes("ms-appx:///Assets/pixeval_server_cert.cer"));
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