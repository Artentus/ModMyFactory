using System;

namespace ModMyFactory.Web.UpdateApi
{
    sealed class UpdateInfo
    {
        public Package Package { get; }

        public UpdateInfo(UpdateInfoTemplate template)
        {
            Package = Environment.Is64BitOperatingSystem
                ? new Package(template.Win64Package)
                : new Package(template.Win32Package);
        }
    }
}
