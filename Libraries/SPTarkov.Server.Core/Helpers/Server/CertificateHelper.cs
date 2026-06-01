using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Helpers.Server;

[Injectable]
public class CertificateHelper(ISptLogger<CertificateHelper> logger, FileUtil fileUtil)
{
    private static readonly string _certificatePath = Path.Combine("user", "certs", "server.crt");
    private static readonly string _certificateKeyPath = Path.Combine("user", "certs", "server.key");

    public X509Certificate2? LoadOrGenerateCertificate()
    {
        if (!Directory.Exists("./user/certs"))
        {
            Directory.CreateDirectory("./user/certs");
        }

        if (TryLoadCertificate(out var cert))
        {
            logger.Success($"Loaded self-signed certificate ({_certificatePath})");
            return cert;
        }

        // shit went wrong, throw a wobbly and close app
        logger.Critical("Certificate could not be loaded. Stopping server...");
        Environment.Exit(1);
        return null;
    }

    /// <summary>
    ///     if the cert exist, try load it, else create one and try load again
    /// </summary>
    /// <returns></returns>
    private bool TryLoadCertificate(out X509Certificate2 certificate)
    {
        if (!File.Exists(_certificatePath) || !File.Exists(_certificateKeyPath))
        {
            // file doesn't exist so create straight away
            var cert = GenerateSelfSignedCertificate();
            var privateKey = cert.GetRSAPrivateKey();

            if (privateKey is null)
            {
                throw new InvalidOperationException("Private key is null");
            }

            SaveCertificate(cert);
            SavePrivateKey(privateKey);

            logger.Success($"Generated and stored self-signed certificate ({_certificatePath})");
        }

        try
        {
            var pem = X509Certificate2.CreateFromPemFile(_certificatePath, _certificateKeyPath);

            // This badness has to exist so that .Net can actually use the cert for HTTPS
            certificate = X509CertificateLoader.LoadPkcs12(pem.Export(X509ContentType.Pfx), null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        // Just return true, we're throwing an exception if creating the cert fails
        return true;
    }

    /// <summary>
    ///     Generate and return a self-signed certificate
    /// </summary>
    /// <returns>X509Certificate2</returns>
    private X509Certificate2 GenerateSelfSignedCertificate()
    {
        var sanBuilder = new SubjectAlternativeNameBuilder();
        sanBuilder.AddIpAddress(IPAddress.Loopback);
        sanBuilder.AddDnsName("localhost");

        var distinguishedName = new X500DistinguishedName("CN=localhost, O=spt-csharp");

        using var rsa = RSA.Create(4096);
        var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(sanBuilder.Build());

        return request.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-1)), new DateTimeOffset(DateTime.UtcNow.AddYears(99)));
    }

    /// <summary>
    ///     Save a certificate as a file to disk
    /// </summary>
    /// <param name="certificate">Certificate to save</param>
    private void SaveCertificate(X509Certificate2 certificate)
    {
        try
        {
            // Save as PEM (ensure the certificate is in PEM format)
            var certPem =
                "-----BEGIN CERTIFICATE-----\n"
                + Convert.ToBase64String(certificate.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks)
                + "\n-----END CERTIFICATE-----";
            fileUtil.WriteFile(_certificatePath, certPem);
        }
        catch (Exception ex)
        {
            logger.Error($"Error saving certificate: {ex.Message}");
        }
    }

    private void SavePrivateKey(RSA privateKey)
    {
        try
        {
            var privateKeyBytes = privateKey.ExportPkcs8PrivateKey();

            // Convert the private key to PEM format (Base64 encoded)
            var privateKeyString =
                "-----BEGIN PRIVATE KEY-----\n"
                + Convert.ToBase64String(privateKeyBytes, Base64FormattingOptions.InsertLineBreaks)
                + "\n-----END PRIVATE KEY-----";

            fileUtil.WriteFile(_certificateKeyPath, privateKeyString);
        }
        catch (Exception ex)
        {
            logger.Error($"Error saving certificate key: {ex.Message}");
        }
    }
}
