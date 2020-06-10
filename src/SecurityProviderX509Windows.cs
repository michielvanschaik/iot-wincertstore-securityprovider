using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Azure.Devices.Shared;

namespace WindowsCertStoreSample
{
    public class SecurityProviderX509Windows : SecurityProviderX509
    {
        private readonly string _registrationId;

        /// <summary>
        /// Initializes a new instance of the SecurityProviderX509Windows class.
        /// </summary>
        /// <param name="registrationId">Name of the certificate used for authentication.</param>
        public SecurityProviderX509Windows(string registrationId)
        {
            _registrationId = registrationId;
        }

        private static X509Certificate2 GetCertificateFromStore(string registrationId)
        {
            X509Store store = new X509Store(StoreLocation.LocalMachine);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                var certs = store.Certificates
                    .Find(X509FindType.FindByTimeValid, DateTime.Now, false)
                    .Find(X509FindType.FindBySubjectDistinguishedName, "CN=" + registrationId, false);
                if (certs.Count > 0)
                {
                    return certs[0];
                }
            }
            finally
            {
                store.Close();
            }
            return null;
        }

        /// <summary>
        /// Gets the certificate trust chain that will end in the Trusted Root installed on the server side.
        /// </summary>
        /// <returns>The certificate chain.</returns>
        public override X509Certificate2 GetAuthenticationCertificate()
        {
            return GetCertificateFromStore(_registrationId);
        }

        /// <summary>
        /// Gets the certificate used for TLS device authentication.
        /// </summary>
        /// <returns>The client certificate used during TLS communications.</returns>
        public override X509Certificate2Collection GetAuthenticationCertificateChain()
        {
            X509Certificate2 certificate = GetCertificateFromStore(_registrationId);

            X509Chain chain = new X509Chain(true);
            chain.Build(certificate);

            X509Certificate2Collection certificateChain = new X509Certificate2Collection();
            foreach (X509ChainElement chainElement in chain.ChainElements)
            {
                certificateChain.Add(chainElement.Certificate);
            }

            return certificateChain;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the SecurityProviderX509Certificate and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to releases only unmanaged resources.</param>
        protected override void Dispose(bool disposing) { }
    }
}