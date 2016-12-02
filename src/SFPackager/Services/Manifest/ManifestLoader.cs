using System.IO;
using System.Xml.Serialization;

namespace SFPackager.Services.Manifest
{
    public class ManifestLoader<T>
    {
        private readonly XmlSerializer _serializer;

        public ManifestLoader()
        {
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("x", "http://schemas.microsoft.com/2011/01/fabric");
            _serializer = new XmlSerializer(typeof(T));
        }

        public T Load(string manifestPath)
        {
            T manifest;
            using (var reader = new FileStream(manifestPath, FileMode.Open))
            {
                manifest = (T) _serializer.Deserialize(reader);
            }

            return manifest;
        }

        public void Save(T manifest, string manifestPath)
        {
            using (var writer = new FileStream(manifestPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                _serializer.Serialize(writer, manifest);
            }
        }

        public Stream Save(T manifest, Stream outStream)
        {
            _serializer.Serialize(outStream, manifest);

            return outStream;
        }
    }
}