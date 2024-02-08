namespace Xperience.Xman.Configuration
{
    /// <summary>
    /// Represents a key from https://docs.xperience.io/xp/developers-and-admins/configuration/reference-configuration-keys.
    /// </summary>
    public class ConfigurationKey
    {
        public string KeyName { get; private set; }


        public string Description { get; private set; }


        public Type ValueType { get; private set; }


        public object? DefaultValue { get; private set; }


        public object? ActualValue { get; set; }


        public ConfigurationKey(string keyName, string description, Type valueType, object? defaultValue = null)
        {
            KeyName = keyName;
            Description = description;
            ValueType = valueType;
            DefaultValue = defaultValue;
        }
    }
}
