using System.Xml.Serialization;

using Xperience.Xman.Options;

namespace Xperience.Xman.Services
{
    public class CDXmlManager : ICDXmlManager
    {
        public async Task<RepositoryConfiguration?> GetConfig(string path)
        {
            string contents = await File.ReadAllTextAsync(path);
            var serializer = new XmlSerializer(typeof(RepositoryConfiguration));
            using var reader = new StringReader(contents);

            return serializer.Deserialize(reader) as RepositoryConfiguration;
        }


        public void WriteConfig(RepositoryConfiguration config, string path)
        {
            var namespaces = new XmlSerializerNamespaces(
            [
                new("xsd", "http://www.w3.org/2001/XMLSchema"),
                new("xsi", "http://www.w3.org/2001/XMLSchema-instance")
            ]);
            var serializer = new XmlSerializer(typeof(RepositoryConfiguration));
            using var writer = new StreamWriter(path);
            serializer.Serialize(writer, config, namespaces);
        }
    }
}
