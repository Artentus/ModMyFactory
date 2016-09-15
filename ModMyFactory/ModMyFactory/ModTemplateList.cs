using System.Collections;
using System.Collections.Generic;

namespace ModMyFactory
{
    class ModTemplateList : IEnumerable<ModTemplate>
    {
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
