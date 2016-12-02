using System;
using System.Linq;
using System.Xml;
using SFPackager.Helpers;
using SFPackager.Models;

namespace SFPackager.Services.Manifest
{
    public class PrincipalAppender : BaseManifestHandler
    {
        private readonly PackageConfig _packageConfig;

        public PrincipalAppender(PackageConfig packageConfig)
        {
            _packageConfig = packageConfig;
        }

        public void SetPrincipals(XmlDocument document, string applicationTypeName)
        {
            var nsManager = GetNsManager(document);

            var principalsForApp = _packageConfig
                .Encipherment
                .Where(x => x.ApplicationTypeName.Equals(applicationTypeName, StringComparison.CurrentCultureIgnoreCase))
                .ToList();

            if (!principalsForApp.Any())
                return;

            var usersElement = document.CreateElement("Users", NamespaceString);

            foreach (var principal in principalsForApp)
            {
                var user = document.CreateElement("User", NamespaceString);
                user.SetAttribute("Name", principal.Name);
                user.SetAttribute("AccountType", "NetworkService");

                usersElement.AppendChild(user);
            }

            var principalElement = document.CreateElement("Principals", NamespaceString);
            principalElement.AppendChild(usersElement);

            var root = document.GetNode("//x:ApplicationManifest", nsManager);
            root.AppendChild(principalElement);
        }
    }
}