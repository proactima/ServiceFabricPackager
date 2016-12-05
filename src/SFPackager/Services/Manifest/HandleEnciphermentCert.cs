using System;
using System.Collections.Generic;
using System.Linq;
using SFPackager.Models;
using SFPackager.Models.Xml;
using SFPackager.Models.Xml.Elements;

namespace SFPackager.Services.Manifest
{
    public class HandleEnciphermentCert
    {
        public void SetEnciphermentCerts(
            PackageConfig packageConfig,
            ApplicationManifest appManifest,
            string appTypeName)
        {
            var encipherments = packageConfig
                .Encipherment
                .Where(x => EncipherNameEqualsAppName(x, appTypeName))
                .ToList();

            if (!encipherments.Any())
                return;

            if (appManifest.Principals == null)
                appManifest.Principals = new Principals();
            if (appManifest.Principals.Users == null)
                appManifest.Principals.Users = new Users();
            if (appManifest.Principals.Users.User == null)
                appManifest.Principals.Users.User = new List<User>();
            if (appManifest.Policies == null)
                appManifest.Policies = new Policies();
            if (appManifest.Policies.SecurityAccessPolicies == null)
                appManifest.Policies.SecurityAccessPolicies = new SecurityAccessPolicies();
            if (appManifest.Policies.SecurityAccessPolicies.SecurityAccessPolicy == null)
                appManifest.Policies.SecurityAccessPolicies.SecurityAccessPolicy = new List<SecurityAccessPolicy>();
            if (appManifest.Certificates == null)
                appManifest.Certificates = new Certificates();
            if (appManifest.Certificates.SecretsCertificate == null)
                appManifest.Certificates.SecretsCertificate = new List<SecretsCertificate>();

            foreach (var encipherment in encipherments)
            {
                var user = new User
                {
                    Name = encipherment.Name,
                    AccountType = "NetworkService"
                };

                var policy = new SecurityAccessPolicy
                {
                    GrantRights = "Read",
                    PrincipalRef = encipherment.Name,
                    ResourceRef = encipherment.CertName,
                    ResourceType = "Certificate"
                };

                var secretCert = new SecretsCertificate
                {
                    Name = encipherment.CertName,
                    X509FindValue = encipherment.CertThumbprint,
                    X509FindType = "FindByThumbprint"
                };

                appManifest.Principals.Users.User.Add(user);
                appManifest.Policies.SecurityAccessPolicies.SecurityAccessPolicy.Add(policy);
                appManifest.Certificates.SecretsCertificate.Add(secretCert);
            }
        }

        private static bool EncipherNameEqualsAppName(
            Encipherment enchipherment,
            string appTypeName)
        {
            return enchipherment.ApplicationTypeName.Equals(appTypeName,
                StringComparison.CurrentCultureIgnoreCase);
        }
    }
}