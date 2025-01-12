// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Security.Cryptography.X509Certificates;

namespace Pixeval.Pages.Login;

public static class CertificateManager
{
    public static void Install(this X509Certificate2 certificate, StoreName storeName, StoreLocation storeLocation)
    {
        using var store = new X509Store(storeName, storeLocation);
        store.Open(OpenFlags.ReadWrite);
        store.Add(certificate);
    }

    public static void Uninstall(this X509Certificate2 certificate, StoreName storeName, StoreLocation storeLocation)
    {
        using var store = new X509Store(storeName, storeLocation);
        store.Open(OpenFlags.ReadWrite);
        store.Remove(certificate);
    }

    public static bool Query(this X509Certificate2 certificate, StoreName storeName, StoreLocation storeLocation)
    {
        using var store = new X509Store(storeName, storeLocation);
        store.Open(OpenFlags.ReadOnly);
        return store.Certificates.Find(X509FindType.FindByThumbprint, certificate.Thumbprint, true).Count > 0;
    }
}
