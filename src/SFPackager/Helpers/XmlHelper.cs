using System.Xml;

namespace SFPackager.Helpers
{
    internal static class XmlHelper
    {
        internal static string GetSingleValue(this XmlNode document, string xPath, XmlNamespaceManager nsManager)
        {
            var node = document.SelectSingleNode(xPath, nsManager);
            return node != null
                ? node.Value
                : string.Empty;
        }

        internal static void SetSingleValue(this XmlNode document, string xPath, string value, XmlNamespaceManager nsManager)
        {
            var node = document.SelectSingleNode(xPath, nsManager);
            node.Value = value;
        }

        internal static XmlNodeList GetNodes(this XmlNode document, string xPath, XmlNamespaceManager nsManager)
        {
            return document.SelectNodes(xPath, nsManager);
        }

        internal static XmlNode GetNode(this XmlNode document, string xPath, XmlNamespaceManager nsManager)
        {
            return document.SelectSingleNode(xPath, nsManager);
        }

        internal static void RemoveNodes(this XmlNode document, string parentXpath, string childXpath, XmlNamespaceManager nsManager)
        {
            var parameterNodes = GetNodes(document, $"{parentXpath}{childXpath}", nsManager);
            var parent = document.SelectSingleNode(parentXpath, nsManager);
            foreach (var parameterNode in parameterNodes)
            {
                var node = parameterNode as XmlNode;
                if (node == null)
                    return;

                parent.RemoveChild(node);
            }
        }
    }
}