using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toml.Net
{
    public class Toml
    {
        private string Location { get; set; }
        private Dictionary<string, TomlKeyValuePair[]> _values; 

        public Toml(string location)
        {
            Location = location;
            _values = new Dictionary<string, TomlKeyValuePair[]>();
        }

        public Toml() : this("config.toml")
        {
            
        }


    }
}
