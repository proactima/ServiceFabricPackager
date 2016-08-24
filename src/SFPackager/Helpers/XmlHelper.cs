using System.Xml;

namespace SFPackager.Helpers
{
    internal static class XmlHelper
    {
        internal static string GetSingleValue(string xPath, XmlNode document, XmlNamespaceManager nsManager)
        {
            var node = document.SelectSingleNode(xPath, nsManager);
            return node != null
                ? node.Value
                : string.Empty;
        }

        internal static void SetSingleValue(string xPath, string value, XmlNode document,
            XmlNamespaceManager nsManager)
        {
            var node = document.SelectSingleNode(xPath, nsManager);
            node.Value = value;
        }

        internal static XmlNodeList GetNodes(string xPath, XmlNode document, XmlNamespaceManager nsManager)
        {
            return document.SelectNodes(xPath, nsManager);
        }

        internal static XmlNode GetNode(string xPath, XmlNode document, XmlNamespaceManager nsManager)
        {
            return document.SelectSingleNode(xPath, nsManager);
        }

        internal static void RemoveNodes(string parentXpath, string childXpath, XmlNode document, XmlNamespaceManager nsManager)
        {
            var parameterNodes = GetNodes($"{parentXpath}{childXpath}", document, nsManager);
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