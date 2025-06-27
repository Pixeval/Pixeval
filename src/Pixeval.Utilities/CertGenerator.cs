using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Pixeval.Utilities;

/// <summary>
/// <seealso href="https://github.com/BeyondDimension/SteamTools/blob/ece1768b2a9388aeef3474421fe16a7ebcf6880b/src/BD.WTTS.Client.Plugins.Accelerator.ReverseProxy/Services.Implementation/Certificate/CertGenerator.cs"/>
/// </summary>
public static class CertGenerator
{
    /// <summary>
    /// 证书名称，硬编码不可改动，确保兼容性
    /// </summary>
    public const string RootCertificateName = $"{nameof(Pixeval)} Certificate";

    public const string X500DistinguishedNameValue = $"C=CN, O=PixevalOrg, CN={RootCertificateName}";

    public static Oid TlsServerOid { get; } = new("1.3.6.1.5.5.7.3.1");

    public static Oid TlsClientOid { get; } = new ("1.3.6.1.5.5.7.3.2");

    public static Task GenerateBySelfPfxAsync(
        string? x509Name,
        DateTimeOffset notBefore,
        DateTimeOffset notAfter,
        string caPfxPath,
        string? password = null)
    {
        var r = CreateCACertificate(x509Name, notBefore, notAfter);
        var exported = r.Export(X509ContentType.Pkcs12, password);
        return File.WriteAllBytesAsync(caPfxPath, exported);
    }

    public static byte[] GenerateBySelfPfx(
        string? x509Name,
        DateTimeOffset notBefore,
        DateTimeOffset notAfter,
        string? password = null)
    {
        var r = CreateCACertificate(x509Name, notBefore, notAfter);
        return r.Export(X509ContentType.Pkcs12, password);
    }

    /// <summary>
    /// 生成自签名ca证书
    /// </summary>
    /// <param name="subjectName"></param>
    /// <param name="notBefore">此证书被视为有效的最早日期和时间。通常是 UtcNow，加上或减去几秒钟</param>
    /// <param name="notAfter">此证书不再被视为有效的日期和时间</param>
    /// <param name="rsaKeySizeInBits"></param>
    /// <param name="pathLengthConstraint"></param>
    /// <returns></returns>
    public static X509Certificate2 CreateCACertificate(
        string? subjectName,
        DateTimeOffset notBefore,
        DateTimeOffset notAfter,
        int rsaKeySizeInBits = 2048,
        int pathLengthConstraint = 1)
    {
        using var rsa = RSA.Create(rsaKeySizeInBits);
        var request = new CertificateRequest(new X500DistinguishedName(string.IsNullOrEmpty(subjectName) ? X500DistinguishedNameValue : subjectName), rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        var basicConstraints = new X509BasicConstraintsExtension(true, pathLengthConstraint > 0, pathLengthConstraint, true);
        request.CertificateExtensions.Add(basicConstraints);

        var keyUsage = new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.CrlSign | X509KeyUsageFlags.KeyCertSign, true);
        request.CertificateExtensions.Add(keyUsage);

        var oids = new OidCollection { TlsServerOid, TlsClientOid };
        var enhancedKeyUsage = new X509EnhancedKeyUsageExtension(oids, true);
        request.CertificateExtensions.Add(enhancedKeyUsage);

        var dnsBuilder = new SubjectAlternativeNameBuilder();
        dnsBuilder.Add(RootCertificateName);
        request.CertificateExtensions.Add(dnsBuilder.Build());

        var subjectKeyId = new X509SubjectKeyIdentifierExtension(request.PublicKey, false);
        request.CertificateExtensions.Add(subjectKeyId);

        return request.CreateSelfSigned(notBefore, notAfter);
    }

    /// <summary>
    /// 生成服务器证书
    /// </summary>
    /// <param name="issuerCertificate"></param>
    /// <param name="subjectName"></param>
    /// <param name="extraDnsNames"></param>
    /// <param name="notBefore"></param>
    /// <param name="notAfter"></param>
    /// <param name="rsaKeySizeInBits"></param>
    /// <returns></returns>
    public static X509Certificate2 CreateEndCertificate(
        X509Certificate2 issuerCertificate,
        X500DistinguishedName subjectName,
        IEnumerable<string>? extraDnsNames = null,
        DateTimeOffset? notBefore = null,
        DateTimeOffset? notAfter = null,
        int rsaKeySizeInBits = 2048)
    {
        using var rsa = RSA.Create(rsaKeySizeInBits);
        var request = new CertificateRequest(subjectName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        var basicConstraints = new X509BasicConstraintsExtension(false, false, 0, true);
        request.CertificateExtensions.Add(basicConstraints);

        var keyUsage = new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, true);
        request.CertificateExtensions.Add(keyUsage);

        var oids = new OidCollection { TlsServerOid, TlsClientOid };
        var enhancedKeyUsage = new X509EnhancedKeyUsageExtension(oids, true);
        request.CertificateExtensions.Add(enhancedKeyUsage);

        var authorityKeyId = GetAuthorityKeyIdentifierExtension(issuerCertificate);
        request.CertificateExtensions.Add(authorityKeyId);

        var subjectKeyId = new X509SubjectKeyIdentifierExtension(request.PublicKey, false);
        request.CertificateExtensions.Add(subjectKeyId);

        var dnsBuilder = new SubjectAlternativeNameBuilder();
        dnsBuilder.Add(subjectName.Name[3..]);

        if (extraDnsNames != null)
        {
            foreach (var dnsName in extraDnsNames)
            {
                dnsBuilder.Add(dnsName);
            }
        }

        var dnsNames = dnsBuilder.Build();
        request.CertificateExtensions.Add(dnsNames);

        if (notBefore is null || notBefore.Value < issuerCertificate.NotBefore)
            notBefore = issuerCertificate.NotBefore;

        if (notAfter is null || notAfter.Value > issuerCertificate.NotAfter)
            notAfter = issuerCertificate.NotAfter;

        var serialNumber = BitConverter.GetBytes(Random.Shared.NextInt64());
        using var certOnly = request.Create(issuerCertificate, notBefore.Value, notAfter.Value, serialNumber);
        return certOnly.CopyWithPrivateKey(rsa);
    }

    private static void Add(this SubjectAlternativeNameBuilder builder, string name)
    {
        if (IPAddress.TryParse(name, out var address))
            builder.AddIpAddress(address);
        else
        {
            try
            {
                builder.AddDnsName(name);
            }
            catch
            {
                /* name = Environment.MachineName
                 * System.ArgumentException: Decoded string is not a valid IDN name. (Parameter 'unicode')
                 * at System.Globalization.IdnMapping.IcuGetAsciiCore(String unicodeString, Char* unicode, Int32 count)
                 * at System.Globalization.IdnMapping.GetAscii(String unicode, Int32 index, Int32 count)
                 * at System.Security.Cryptography.X509Certificates.SubjectAlternativeNameBuilder.AddDnsName(String dnsName)
                 * at BD.WTTS.Services.Implementation.CertGenerator.Add(SubjectAlternativeNameBuilder builder, String name)
                 * at BD.WTTS.Services.Implementation.CertGenerator.CreateEndCertificate(X509Certificate2 issuerCertificate, X500DistinguishedName subjectName, IEnumerable`1 extraDnsNames, Nullable`1 notBefore, Nullable`1 notAfter, Int32 rsaKeySizeInBits)
                 */
            }
        }
    }

    private static X509AuthorityKeyIdentifierExtension GetAuthorityKeyIdentifierExtension(X509Certificate2 certificate)
    {
        var extension = new X509SubjectKeyIdentifierExtension(certificate.PublicKey, false);
        return X509AuthorityKeyIdentifierExtension.CreateFromSubjectKeyIdentifier(extension);
    }
}
