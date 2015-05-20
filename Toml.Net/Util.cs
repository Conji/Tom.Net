using System.Linq;

namespace Toml.Net
{
    public static class Util
    {
        public static object Get(this TomlKeyValuePair[] pairs, string key)
        {
            return pairs.Single(p => p.Key == key).Value;
        }

        public static T Get<T>(this TomlKeyValuePair[] pairs, string key)
        {
            return (T) pairs.Single(p => p.Key == key && p.Value.GetType() == typeof (T)).Value;
        }

        public static bool HasKey(this TomlKeyValuePair[] pairs, string key)
        {
            return pairs.Any(p => p.Key == key);
        }
    }
}
