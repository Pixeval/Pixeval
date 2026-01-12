// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Security.Cryptography.X509Certificates;

namespace Pixeval.Pages.Login;

public static class CertificateManager
{
    extension(X509Certificate2 certificate)
    {
        public void Install(StoreName storeName, StoreLocation storeLocation)
        {
            using var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadWrite);
            store.Add(certificate);
        }

        public void Uninstall(StoreName storeName, StoreLocation storeLocation)
        {
            using var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadWrite);
            store.Remove(certificate);
        }

        public bool Query(StoreName storeName, StoreLocation storeLocation)
        {
            using var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadOnly);
            return store.Certificates.Find(X509FindType.FindByThumbprint, certificate.Thumbprint, true).Count > 0;
        }
    }
}
