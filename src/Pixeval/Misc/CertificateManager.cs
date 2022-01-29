#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/CertificateManager.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System.Security.Cryptography.X509Certificates;

namespace Pixeval.Misc;

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