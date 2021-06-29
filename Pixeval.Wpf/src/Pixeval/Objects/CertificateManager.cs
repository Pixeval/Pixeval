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
using Pixeval.Objects.Primitive;

namespace Pixeval.Objects
{
    public class CertificateManager
    {
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
            if (Application.GetResourceStream(new Uri("pack://application:,,,/Pixeval;component/Resources/pixeval_ca.cer")) is { } streamResource)
            {
                await using (streamResource.Stream)
                {
                    return new X509Certificate2(await streamResource.Stream.ToBytes());
                }
            }

            throw new FileNotFoundException(Pixeval.Resources.Resources.CannotFindSpecifiedCertificate);
        }

        /// <summary>
        ///     Get the server certificate of Pixeval
        /// </summary>
        /// <returns></returns>
        public static async Task<X509Certificate2> GetFakeServerCertificate()
        {
            if (Application.GetResourceStream(new Uri("pack://application:,,,/Pixeval;component/Resources/pixeval_server_cert.pfx")) is { } streamResource)
            {
                await using (streamResource.Stream)
                {
                    return new X509Certificate2(await streamResource.Stream.ToBytes(), "pixeval", X509KeyStorageFlags.UserKeySet);
                }
            }

            throw new FileNotFoundException(Pixeval.Resources.Resources.CannotFindSpecifiedCertificate);
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