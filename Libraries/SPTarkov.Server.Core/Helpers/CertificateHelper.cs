using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Helpers
{
    [Injectable]
    public class CertificateHelper(ISptLogger<CertificateHelper> _logger, FileUtil _fileUtil)
    {
        private const string certificatePath = "./user/certs/server.crt";
        private const string certificateKeyPath = "./user/certs/server.key";
        private const string certificatePfxPath = "./user/certs/certificate.pfx";

        //Todo: Finish off to match TS server
        /// <summary>
        /// Currently not in use
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public X509Certificate2 LoadOrGenerateCertificate()
        {
            if (!Directory.Exists("./user/certs"))
            {
                Directory.CreateDirectory("./user/certs");
            }

            var certificate = LoadCertificate();

            if (certificate == null)
            {
                // Generate self-signed certificate
                certificate = GenerateSelfSignedCertificate("localhost");
                SaveCertificate(certificate); // Save cert and new key
                certificate = LoadCertificate();
                if (certificate == null)
                {
                    // if we are still null here there is a serious problem creating cert
                    throw new Exception("Certificate could not be loaded for the second time.");
                }

                _logger.Success($"Generated and stored self-signed certificate ({certificatePath})");
            }

            return certificate;
        }

        //Todo: When the above is finished off, remove any method with Pfx in the name
        public X509Certificate2 LoadOrGenerateCertificatePfx()
        {
            if (!Directory.Exists("./user/certs"))
            {
                Directory.CreateDirectory("./user/certs");
            }

            if (TryLoadCertificatePfx(out var cert))
            {
                _logger.Success($"Loaded self-signed certificate ({certificatePath})");
                return cert;
            }
            else
            {
                // shit went wrong, throw a wobbly and close app
                _logger.Critical("Certificate pfx could not be loaded. Stopping server...");
                Environment.Exit(1);
                return null;
            }
        }

        private X509Certificate2? LoadCertificate()
        {
            try
            {
                return X509Certificate2.CreateFromPemFile(certificatePath, certificateKeyPath);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// if the cert exist, try load it, else create one and try load again
        /// </summary>
        /// <returns></returns>
        private bool TryLoadCertificatePfx(out X509Certificate2? certificate)
        {
            X509Certificate2 cert = null;
            if (!File.Exists(certificatePfxPath))
            {
                // file doesnt exist so create straight away
                cert = GenerateSelfSignedCertificate("localhost");
                SaveCertificatePfx(cert);
                _logger.Success($"Generated and stored self-signed certificate ({certificatePath})");
            }

            try
            {
                //Archangel: For some reason despite this being deprecated this is the only way to load a certificate file
                //No idea why, I want to eventually switch over to the other format so it lines up with the TS server
                //But for now this works fine
                certificate = new X509Certificate2(certificatePfxPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            if (certificate is not null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get a certificate from provided path and return
        /// </summary>
        /// <returns>X509Certificate2</returns>
        private X509Certificate2? LoadCertificatePfx()
        {
            try
            {
                //Archangel: For some reason despite this being deprecated this is the only way to load a certificate file
                //No idea why, I want to eventually switch over to the other format so it lines up with the TS server
                //But for now this works fine
                return new(certificatePfxPath);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Generate and return a self-signed certificate
        /// </summary>
        /// <param name="subjectName">e.g. localhost</param>
        /// <returns>X509Certificate2</returns>
        private X509Certificate2 GenerateSelfSignedCertificate(string subjectName)
        {
            var sanBuilder = new SubjectAlternativeNameBuilder();
            sanBuilder.AddIpAddress(IPAddress.Loopback);
            sanBuilder.AddDnsName("localhost");
            sanBuilder.AddDnsName(Environment.MachineName);

            var distinguishedName = new X500DistinguishedName($"CN={subjectName}");

            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            request.CertificateExtensions.Add(sanBuilder.Build());

            //Todo: Enable when Pfx methods can be removed
            //SavePrivateKey(rsa);

            return request.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-1)), new DateTimeOffset(DateTime.UtcNow.AddDays(3650)));
        }

        /// <summary>
        /// Save a certificate as a file to disk
        /// </summary>
        /// <param name="certificate">Certificate to save</param>
        private void SaveCertificate(X509Certificate2 certificate)
        {
            try
            {
                // Save as PEM (ensure the certificate is in PEM format)
                var certPem = "-----BEGIN CERTIFICATE-----\n" +
                              Convert.ToBase64String(certificate.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks) +
                              "\n-----END CERTIFICATE-----";
                _fileUtil.WriteFile(certificatePath, certPem);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error saving certificate: {ex.Message}");
            }
        }

        /// <summary>
        /// Save a certificate as a file to disk
        /// </summary>
        /// <param name="certificate">Certificate to save</param>
        private void SaveCertificatePfx(X509Certificate2 certificate)
        {
            try
            {
                _fileUtil.WriteFile(certificatePfxPath, certificate.Export(X509ContentType.Pfx));
            }
            catch (Exception ex)
            {
                _logger.Error($"Error saving certificate: {ex.Message}");
            }
        }

        private void SavePrivateKey(RSA privateKey)
        {
            try
            {
                var privateKeyBytes = privateKey.ExportPkcs8PrivateKey();

                // Convert the private key to PEM format (Base64 encoded)
                var privateKeyString = "-----BEGIN PRIVATE KEY-----\n" +
                                       Convert.ToBase64String(privateKeyBytes, Base64FormattingOptions.InsertLineBreaks) +
                                       "\n-----END PRIVATE KEY-----";

                _fileUtil.WriteFile(certificateKeyPath, privateKeyString);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error saving certificate key: {ex.Message}");
            }
        }
    }
}
