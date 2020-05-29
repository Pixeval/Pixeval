#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows;
using Pixeval.Objects.I18n;
using Pixeval.Objects.Primitive;

namespace Pixeval.Persisting.WebApi
{
    /// <summary>
    ///     This class is for web-API login usage only, the private key of the CA Root Certificate
    ///     is personally guaranteed to be absolutely secure. However, if you see any warning or any
    ///     website is using the certificate which is issued by "Dylech30th Do Not Trust Certificate
    ///     Authority", <strong>PLEASE NEVER TRUST THEM</strong>. Suppress this warning may lead into
    ///     a Man-In-The-Middle attack, and I personally will not be responsible for any consequence
    ///     that caused by suppressing this warning. USE AT YOUR OWN RISK
    /// </summary>
    public class CertificateManager
    {
        private static X509Certificate2 _fakeCa;
        private static X509Certificate2 _serverCert;

        private readonly X509Certificate2 cert;

        /// <summary>
        ///     Create a <see cref="CertificateManager" /> with specified <see cref="X509Certificate2" />
        /// </summary>
        /// <param name="certificate">certificate to be managed</param>
        public CertificateManager(X509Certificate2 certificate)
        {
            cert = certificate;
        }

        /// <summary>
        ///     Get the fake CA root certificate of Pixeval
        /// </summary>
        /// <returns></returns>
        public static async Task<X509Certificate2> GetFakeCaRootCertificate()
        {
            if (_fakeCa != null) return _fakeCa;
            if (Application.GetResourceStream(
                new Uri("pack://application:,,,/Pixeval;component/Resources/pixeval_ca.cer")) is { } streamResource)
            {
                await using (streamResource.Stream)
                {
                    _fakeCa = new X509Certificate2(await streamResource.Stream.ToBytes());
                }

                return _fakeCa;
            }

            throw new FileNotFoundException(AkaI18N.CannotFindSpecifiedCertificate);
        }

        /// <summary>
        ///     Get the server certificate of Pixeval
        /// </summary>
        /// <returns></returns>
        public static async Task<X509Certificate2> GetFakeServerCertificate()
        {
            if (_serverCert != null) return _serverCert;
            if (Application.GetResourceStream(
                    new Uri("pack://application:,,,/Pixeval;component/Resources/pixeval_server_cert.pfx")) is { }
                streamResource)
            {
                await using (streamResource.Stream)
                {
                    _serverCert = new X509Certificate2(await streamResource.Stream.ToBytes(), "pixeval", X509KeyStorageFlags.EphemeralKeySet);
                }

                return _serverCert;
            }

            throw new FileNotFoundException(AkaI18N.CannotFindSpecifiedCertificate);
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
            store.Add(cert);
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
            store.Remove(cert);
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
            if (cert.Thumbprint == null) return false;
            return store.Certificates.Find(X509FindType.FindByThumbprint, cert.Thumbprint, true).Count > 0;
        }
    }
}