using System.Xml.Serialization;

namespace Xperience.Xman.Options
{
    /// <summary>
    /// Represents the CD configuration XML file.
    /// </summary>
    public class RepositoryConfiguration : IWizardOptions
    {
        public string? RestoreMode { get; set; }


        [XmlArray]
        [XmlArrayItem(ElementName = "ObjectType")]
        public List<string>? IncludedObjectTypes { get; set; }


        [XmlArray]
        [XmlArrayItem(ElementName = "ObjectType")]
        public List<string>? ExcludedObjectTypes { get; set; }
    }
}
