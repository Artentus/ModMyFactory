using System;
using System.Collections.Generic;
using System.IO;

namespace ModMyFactory
{
    static class ModManager
    {
        static readonly List<ModTemplateList> TemplateLists;

        static ModManager()
        {
            TemplateLists = new List<ModTemplateList>();

            var modDirectory = App.Instance.Settings.GetModDirectory();
            if (modDirectory.Exists)
            {
                foreach (var directory in modDirectory.EnumerateDirectories())
                {
                    Version version;
                    if (Version.TryParse(directory.Name, out version))
                    {
                        var templateList = ModTemplateList.Load(Path.Combine(directory.FullName, "mod-list.json"));
                        templateList.Version = version;
                        TemplateLists.Add(templateList);
                    }
                }
            }
        }

        private static bool Contains(Version version, out ModTemplateList list)
        {
            list = TemplateLists.Find(l => l.Version == version);
            return list != null;
        }

        public static bool GetActive(string name, Version version)
        {
            ModTemplateList list;
            if (Contains(version, out list))
            {
                return list.GetActive(name);
            }
            else
            {
                list = ModTemplateList.Load(Path.Combine(App.Instance.Settings.GetModDirectory(version).FullName, "mod-list.json"));
                list.Version = version;
                TemplateLists.Add(list);

                return list.GetActive(name);
            }
        }

        public static void SetActive(string name, Version version, bool value)
        {
            ModTemplateList list;
            if (Contains(version, out list))
            {
                list.SetActive(name, value);
            }
            else
            {
                list = ModTemplateList.Load(Path.Combine(App.Instance.Settings.GetModDirectory(version).FullName, "mod-list.json"));
                list.Version = version;
                TemplateLists.Add(list);

                list.SetActive(name, value);
            }
        }

        /// <summary>
        /// Removes a mods template.
        /// </summary>
        /// <param name="name">The mods name.</param>
        public static void RemoveTemplate(string name)
        {
            TemplateLists.ForEach(list => list.Remove(name));
        }

        /// <summary>
        /// Starts updating all mod templates.
        /// While updating all save commands will be ignored.
        /// </summary>
        public static void BeginUpdateTemplates()
        {
            TemplateLists.ForEach(list => list.BeginUpdate());
        }

        /// <summary>
        /// Finishes updating all mod templates.
        /// </summary>
        /// <param name="force">If true forces to end updating, otherwise EndUpdate will have to be called as many times as BeginUpdate has been called.</param>
        public static void EndUpdateTemplates(bool force = false)
        {
            TemplateLists.ForEach(list => list.EndUpdate(force));
        }

        /// <summary>
        /// Saves all mod templates to their files.
        /// </summary>
        public static void SaveTemplates()
        {
            TemplateLists.ForEach(list => list.Save());
        }
    }
}
