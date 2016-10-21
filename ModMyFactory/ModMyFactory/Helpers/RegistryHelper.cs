using System;
using System.IO;
using Microsoft.Win32;

namespace ModMyFactory.Helpers
{
    static class RegistryHelper
    {
        public static string RegisterHandler(string component, int version, string description, string iconPath)
        {
            if (string.IsNullOrEmpty(component)) throw new ArgumentNullException(nameof(component));

            string progId = string.Join(".", "ModMyFactory", component, version);
            RegistryKey handlerKey = Registry.CurrentUser.CreateSubKey(Path.Combine(@"Software\Classes", progId));

            if (handlerKey != null)
            {
                if (!string.IsNullOrEmpty(description))
                {
                    handlerKey.SetValue(null, description, RegistryValueKind.String);
                    handlerKey.SetValue("FriendlyTypeName", description, RegistryValueKind.String);
                }

                if (!string.IsNullOrEmpty(iconPath))
                {
                    RegistryKey iconKey = handlerKey.CreateSubKey("DefaultIcon");
                    iconKey?.SetValue(null, iconPath, RegistryValueKind.String);
                }
            }

            return progId;
        }

        public static void RegisterFileType(string extension, string handlerName, string mimeType, PercievedFileType percievedType)
        {
            if (string.IsNullOrEmpty(extension)) throw new ArgumentNullException(nameof(extension));
            if (string.IsNullOrEmpty(handlerName)) throw new ArgumentNullException(nameof(handlerName));

            if (!extension.StartsWith(".")) extension = "." + extension;

            RegistryKey extensionKey = Registry.CurrentUser.CreateSubKey(Path.Combine(@"Software\Classes", extension));

            if (extensionKey != null)
            {
                extensionKey.SetValue(null, handlerName);

                if (!string.IsNullOrEmpty(mimeType))
                    extensionKey.SetValue("Content Type", mimeType, RegistryValueKind.String);

                if (percievedType != PercievedFileType.None)
                    extensionKey.SetValue("PerceivedType", percievedType.ToString("g"), RegistryValueKind.String);
            }
        }
    }
}
