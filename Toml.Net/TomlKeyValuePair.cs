namespace Toml.Net
{
    public struct TomlKeyValuePair
    {
        public string Key;
        public object Value;

        public TomlKeyValuePair(string key, object value)
        {
            Key = key;
            Value = value;
        }
    }
}
