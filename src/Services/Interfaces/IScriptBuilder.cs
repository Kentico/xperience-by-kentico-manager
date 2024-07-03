namespace Xperience.Manager.Services
{
    /// <summary>
    /// Contains methods for generating scripts to execute with <see cref="IShellRunner"/>.
    /// </summary>
    public interface IScriptBuilder : IService
    {
        /// <summary>
        /// Appends " --cloud" to the script if <paramref name="useCloud"/> is true.
        /// </summary>
        public IScriptBuilder AppendCloud(bool useCloud);


        /// <summary>
        /// Appends the directory to create if the script is <see cref="ScriptType.CreateDirectory"/>.
        /// </summary>
        /// <param name="path">The path of the directory to create.</param>
        public IScriptBuilder AppendDirectory(string? path);


        /// <summary>
        /// Appends the namespace to use if the script is <see cref="ScriptType.GenerateCode"/> and <paramref name="nameSpace"/>
        /// is not empty.
        /// </summary>
        public IScriptBuilder AppendNamespace(string? nameSpace);


        /// <summary>
        /// Appends "--old-salt" and/or "--new-salt" to the script if the script is <see cref="ScriptType.ResignMacros"/>.
        /// </summary>
        /// <param name="oldSalt">The old salt value appended to the script.</param>
        /// <param name="newSalt">The new salt value appended to the script. If not provided, the salt from appsettings is used.</param>
        public IScriptBuilder AppendSalts(string? oldSalt, string? newSalt);


        /// <summary>
        /// Appends "--sign-all" and "--username" to the script if the script is <see cref="ScriptType.ResignMacros"/> and
        /// <paramref name="signAll"/> is <c>true</c>.
        /// </summary>
        public IScriptBuilder AppendSignAll(bool signAll, string? userName);


        /// <summary>
        /// Appends a version number to the script if <paramref name="version"/> is not null.
        /// </summary>
        public IScriptBuilder AppendVersion(Version? version);


        /// <summary>
        /// Gets the generated script.
        /// </summary>
        public string Build();


        /// <summary>
        /// Initializes a new instance of <see cref="ScriptBuilder"/>.
        /// </summary>
        /// <param name="type">The type of script to generate.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public IScriptBuilder SetScript(ScriptType type);


        /// <summary>
        /// Replaces script placeholders with the values of the object properties. If a property is <c>null</c> or emtpy,
        /// the placeholder remains in the script.
        /// </summary>
        public IScriptBuilder WithPlaceholders(object? dataObject);
    }
}
