using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ModMyFactory
{
    [DataContract]
    class ModTemplateList : IEnumerable<ModTemplate>
    {
        [DataMember(Name = "mods")]
        public ModTemplate[] Mods;

        public IEnumerator<ModTemplate> GetEnumerator()
        {
            return ((IEnumerable<ModTemplate>)Mods).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Mods.GetEnumerator();
        }
    }
}
