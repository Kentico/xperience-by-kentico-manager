using Xperience.Manager.Commands;

namespace Xperience.Manager.Options
{
    /// <summary>
    /// The options used to generate code files in <see cref="CodeGenerateCommand"/>. See
    /// <see href="https://docs.xperience.io/xp/developers-and-admins/api/generate-code-files-for-system-objects#Generatecodefilesforsystemobjects-Generatecodefiles"/>. 
    /// </summary>
    public class CodeGenerateOptions : IWizardOptions
    {
        public const string TYPE_FORMS = "Forms";
        public const string TYPE_REUSABLE_CONTENT_TYPES = "ReusableContentTypes";
        public const string TYPE_PAGE_CONTENT_TYPES = "PageContentTypes";
        public const string TYPE_REUSABLE_FIELD_SCHEMAS = "ReusableFieldSchemas";
        public const string TYPE_CLASSES = "Classes";


        /// <summary>
        /// The types of objects to generate code files for.
        /// </summary>
        public string? Type { get; set; }


        /// <summary>
        /// The relative location to the installation folder to generate code files in.
        /// </summary>
        public string Location { get; set; } = "/{type}/{dataClassNamespace}/{name}";


        /// <summary>
        /// Pattern to limit the included objects, separated by semicolon.
        /// </summary>
        public string Include { get; set; } = "*";


        /// <summary>
        /// Pattern to define the excluded objects, separated by semicolon.
        /// </summary>
        public string Exclude { get; set; } = "test";


        /// <summary>
        /// Controls whether a corresponding provider class *Provider and interface I*Provider is generated for the selected object types.
        /// </summary>
        public bool WithProviderClass { get; set; } = true;


        /// <summary>
        /// The custom namespace added to the code of the generated classes. If not provided, a default system namespace is used based on the generated object type.
        /// </summary>
        public string? Namespace { get; set; }
    }
}
