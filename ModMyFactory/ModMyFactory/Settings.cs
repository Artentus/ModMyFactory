using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ModMyFactory
{
    [DataContract]
    class Settings
    {
        [DataMember(Name = "mod_directory")]
        public string ModDirectory;
    }
}
