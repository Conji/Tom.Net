using System;
using System.Collections.Generic;

namespace Toml.Net
{
    public class Toml
    {
        private string Location { get; set; }
        private readonly Dictionary<string, TomlKeyValuePair[]> _values; 

        public Toml(string location)
        {
            Location = location;
            _values = new Dictionary<string, TomlKeyValuePair[]>();
            
        }

        public Toml() : this("config.toml")
        {
            
        }

        public Toml(string location, Dictionary<string, TomlKeyValuePair[]> values) : this(location)
        {
            _values = values;
        }

        public static Toml Open(string location)
        {
            
        }

        public TomlKeyValuePair[] this[string table] => _values[table];
    }
}
