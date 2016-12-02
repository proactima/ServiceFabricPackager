using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using SFPackager.Helpers;
using SFPackager.Models;

namespace SFPackager.Services.Manifest
{
    public class PolicyAppender : BaseManifestHandler
    {
        private readonly PackageConfig _packageConfig;

        public PolicyAppender(PackageConfig packageConfig)
        {
            _packageConfig = packageConfig;
        }

        public void SetPolicies(XmlDocument document, string applicationTypeName)
        {
            var nsManager = GetNsManager(document);

            var securityPoliciesForApp = _packageConfig
                .Encipherment
                .Where(x => x.ApplicationTypeName.Equals(applicationTypeName, StringComparison.CurrentCultureIgnoreCase))
                .ToList();

            if (!securityPoliciesForApp.Any())
                return;

            var securityElement = document.CreateElement("SecurityAccessPolicies", NamespaceString);

            foreach (var accessPolicy in securityPoliciesForApp)
            {
                var accessElement = document.CreateElement("SecurityAccessPolicy", NamespaceString);
                accessElement.SetAttribute("GrantRights", "Read");
                accessElement.SetAttribute("PrincipalRef", accessPolicy.Name);
                accessElement.SetAttribute("ResourceRef", accessPolicy.CertName);
                accessElement.SetAttribute("ResourceType", "Certificate");

                securityElement.AppendChild(accessElement);
            }

            var policiesElement = document.CreateElement("Policies", NamespaceString);
            policiesElement.AppendChild(securityElement);

            var root = document.GetNode("//x:ApplicationManifest", nsManager);
            root.AppendChild(policiesElement);
        }
    }
}
