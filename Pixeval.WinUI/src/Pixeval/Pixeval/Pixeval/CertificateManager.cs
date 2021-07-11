using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Pixeval
{
    public class CertificateManager
    {
        private readonly X509Certificate2 _cert;

        /// <summary>
        ///     Create a <see cref="CertificateManager" /> with specified <see cref="X509Certificate2" />
        /// </summary>
        /// <param name="certificate">certificate to be managed</param>
        public CertificateManager(X509Certificate2 certificate)
        {
            _cert = certificate;
        }

        /// <summary>
        ///     Get the fake CA root certificate of Pixeval
        /// </summary>
        /// <returns></returns>
        public static async Task<X509Certificate2> GetFakeCaRootCertificate()
        {
            return new(await App.GetResourceBytes("ms-appx:///Assets/pixeval_ca.cer"));
        }

        /// <summary>
        ///     Get the server certificate of Pixeval
        /// </summary>
        /// <returns></returns>
        public static async Task<X509Certificate2> GetFakeServerCertificate()
        {
            return new(await App.GetResourceBytes("ms-appx:///Assets/pixeval_server_cert.cer"));
        }

        /// <summary>
        ///     Install a certificate to specified location
        /// </summary>
        /// <param name="storeName"></param>
        /// <param name="storeLocation"></param>
        public void Install(StoreName storeName, StoreLocation storeLocation)
        {
            using var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadWrite);
            store.Add(_cert);
        }

        /// <summary>
        ///     Uninstall a certificate from specified location
        /// </summary>
        /// <param name="storeName"></param>
        /// <param name="storeLocation"></param>
        public void Uninstall(StoreName storeName, StoreLocation storeLocation)
        {
            using var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadWrite);
            store.Remove(_cert);
        }

        /// <summary>
        ///     Query a certificate from specified location
        /// </summary>
        /// <param name="storeName"></param>
        /// <param name="storeLocation"></param>
        /// <returns>true if the certificate is present</returns>
        public bool Query(StoreName storeName, StoreLocation storeLocation)
        {
            using var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadOnly);
            return store.Certificates.Find(X509FindType.FindByThumbprint, _cert.Thumbprint, true).Count > 0;
        }
    }
}