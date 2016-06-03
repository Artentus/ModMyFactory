using System.Runtime.Serialization;

namespace ModMyFactory
{
    [DataContract]
    class ModTemplate
    {
        [DataMember(Name = "name")]
        public string Name;

        [DataMember(Name = "enabled")]
        public bool Enabled;
    }
}
