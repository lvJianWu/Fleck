using CERTENROLLLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Trusit.Certs.Sign
{
    public class CertManager
    {
        static private string GetAppId()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            //The following line (part of the original answer) is misleading.
            //**Do not** use it unless you want to return the System.Reflection.Assembly type's GUID.
            //Console.WriteLine(assembly.GetType().GUID.ToString());

            // The following is the correct code.
            var attribute = (GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
            var id = attribute.Value;
            return id;
        }
        static public X509Certificate2 GetCert(string cn, TimeSpan expirationLength, string pwd = "", string filename = null)
        {
            // http://stackoverflow.com/questions/18339706/how-to-create-self-signed-certificate-programmatically-for-wcf-service
            // http://stackoverflow.com/questions/21629395/http-listener-with-https-support-coded-in-c-sharp
            // https://msdn.microsoft.com/en-us/library/system.security.cryptography.x509certificates.storename(v=vs.110).aspx
            // create DN for subject and issuer
            var base64encoded = string.Empty;
            if (filename != null && File.Exists(filename))
            {
                base64encoded = File.ReadAllText(filename);
            }
            else
            {
                base64encoded = CreateCertContent(cn, expirationLength, pwd);
                if (filename != null)
                {
                    File.WriteAllText(filename, base64encoded);
                }
            }
            // instantiate the target class with the PKCS#12 data (and the empty password)
            var rlt = new System.Security.Cryptography.X509Certificates.X509Certificate2(
                System.Convert.FromBase64String(base64encoded), pwd,
               // mark the private key as exportable (this is usually what you want to do)
               // mark private key to go into the Machine store instead of the current users store
               X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet
                );
            return rlt;
        }

        private static string CreateCertContent(string cn, TimeSpan expirationLength, string pwd)
        {
            string base64encoded = string.Empty;
            var dn = new CERTENROLLLib.CX500DistinguishedName();
            dn.Encode("CN=" + cn, X500NameFlags.XCN_CERT_NAME_STR_NONE);



            CX509PrivateKey privateKey = new CX509PrivateKey();
            privateKey.ProviderName = "Microsoft Strong Cryptographic Provider";
            privateKey.Length = 2048;
            privateKey.KeySpec = X509KeySpec.XCN_AT_KEYEXCHANGE;
            privateKey.KeyUsage = X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_DECRYPT_FLAG |
                                  X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_KEY_AGREEMENT_FLAG;
            privateKey.MachineContext = true;
            privateKey.ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_PLAINTEXT_EXPORT_FLAG;
            privateKey.Create();

            // Use the stronger SHA512 hashing algorithm
            var hashobj = new CObjectId();
            hashobj.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID,
                ObjectIdPublicKeyFlags.XCN_CRYPT_OID_INFO_PUBKEY_ANY,
                AlgorithmFlags.AlgorithmFlagsNone, "SHA512");

            // Create the self signing request
            var cert = new CX509CertificateRequestCertificate();
            cert.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
            cert.Subject = dn;
            cert.Issuer = dn; // the issuer and the subject are the same
            cert.NotBefore = DateTime.Now.Date;





            var objExtensionAlternativeNames = new CX509ExtensionAlternativeNames();
            {
                var altNames = new CAlternativeNames();
                //var dnsHostname = new CAlternativeName();
                //dnsHostname.InitializeFromString(AlternativeNameType.XCN_CERT_ALT_NAME_DNS_NAME, Environment.MachineName);
                //altNames.Add(dnsHostname);
                //foreach (var ipAddress in Dns.GetHostAddresses(Dns.GetHostName()))
                //{
                //    if ((ipAddress.AddressFamily == AddressFamily.InterNetwork ||
                //         ipAddress.AddressFamily == AddressFamily.InterNetworkV6) && !IPAddress.IsLoopback(ipAddress))
                //    {
                //        var dns = new CAlternativeName();
                //        dns.InitializeFromString(AlternativeNameType.XCN_CERT_ALT_NAME_DNS_NAME, ipAddress.ToString());
                //        altNames.Add(dns);
                //    }
                //}
                var dns1 = new CAlternativeName();
                dns1.InitializeFromString(AlternativeNameType.XCN_CERT_ALT_NAME_DNS_NAME, "localhost");
                altNames.Add(dns1);
                objExtensionAlternativeNames.InitializeEncode(altNames);
            }

            cert.X509Extensions.Add((CX509Extension)objExtensionAlternativeNames);

            // this cert expires immediately. Change to whatever makes sense for you
            cert.NotAfter = cert.NotBefore + expirationLength;
            cert.HashAlgorithm = hashobj; // Specify the hashing algorithm
            cert.Encode(); // encode the certificate

            // Do the final enrollment process
            var enroll = new CX509Enrollment();
            enroll.InitializeFromRequest(cert); // load the certificate
            enroll.CertificateFriendlyName = cn; // Optional: add a friendly name

            string csr = enroll.CreateRequest(); // Output the request in base64
            // and install it back as the response
            enroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate,
                csr, EncodingType.XCN_CRYPT_STRING_BASE64, pwd); // no password
            // output a base64 encoded PKCS#12 so we can import it back to the .Net security classes
            base64encoded = enroll.CreatePFX(pwd, // no password, this is for internal consumption
                PFXExportOptions.PFXExportChainWithRoot);
            return base64encoded;
        }

        public static void ActivateCert(X509Certificate2 rlt)
        {
            X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);
            if (!store.Certificates.Contains(rlt))
            {
                store.Add(rlt);


            }
            store.Close();




            //psi.Arguments = $"http delete sslcert ipport=[::]:{port}";
            //Process procDelV6 = Process.Start(psi);
            //procDelV6.WaitForExit();

            //psi.Arguments = $"http add sslcert ipport=[::]:{port} certhash={rlt.Thumbprint} appid={{{appId}}}";
            //Process procV6 = Process.Start(psi);
            //procV6.WaitForExit();

            //psi.Arguments = $"http add urlacl url=https://+:{port}/ user={Environment.UserDomainName}\\{Environment.UserName}";
            //Process procAcl = Process.Start(psi);
            //procAcl.WaitForExit();
        }
    }
}
