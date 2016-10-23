using System;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using ModMyFactory.Win32;

namespace ModMyFactory.Helpers
{
    static class RegistryHelper
    {
        public static string RegisterHandler(string component, int version, string description, string iconPath)
        {
            if (string.IsNullOrEmpty(component)) throw new ArgumentNullException(nameof(component));

            bool changed = false;

            string progId = string.Join(".", "ModMyFactory", component, version);
            RegistryKey handlerKey = Registry.CurrentUser.CreateSubKey(Path.Combine(@"Software\Classes", progId));

            if (handlerKey != null)
            {
                if (!string.IsNullOrEmpty(description))
                {
                    if ((string)handlerKey.GetValue(null) != description)
                    {
                        handlerKey.SetValue(null, description, RegistryValueKind.String);
                        changed = true;
                    }
                    if ((string)handlerKey.GetValue("FriendlyTypeName") != description)
                    {
                        handlerKey.SetValue("FriendlyTypeName", description, RegistryValueKind.String);
                        changed = true;
                    }
                }

                if (!string.IsNullOrEmpty(iconPath))
                {
                    RegistryKey iconKey = handlerKey.CreateSubKey("DefaultIcon");
                    if ((iconKey != null) && ((string)iconKey.GetValue(null) != iconPath))
                    {
                        iconKey.SetValue(null, iconPath, RegistryValueKind.String);
                        changed = true;
                    }
                }

                string appPath = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
                string command = $"\"{appPath}\" \"%1\"";
                RegistryKey openKey = handlerKey.CreateSubKey(@"shell\open\command");
                if ((openKey != null) && ((string)openKey.GetValue(null) != command))
                {
                    openKey.SetValue(null, command);
                    changed = true;
                }
            }

            if (changed) Shell32.ChangeNotify(ChangeNotifyEventId.AssociationChanged, ChangeNotifyFlags.IdList);

            return progId;
        }

        public static void RegisterFileType(string extension, string handlerName, string mimeType, PercievedFileType percievedType)
        {
            if (string.IsNullOrEmpty(extension)) throw new ArgumentNullException(nameof(extension));
            if (string.IsNullOrEmpty(handlerName)) throw new ArgumentNullException(nameof(handlerName));

            if (!extension.StartsWith(".")) extension = "." + extension;

            bool changed = false;

            RegistryKey extensionKey = Registry.CurrentUser.CreateSubKey(Path.Combine(@"Software\Classes", extension));

            if (extensionKey != null)
            {
                extensionKey.SetValue(null, handlerName);

                if (!string.IsNullOrEmpty(mimeType) && ((string)extensionKey.GetValue("Content Type") != mimeType))
                {
                    extensionKey.SetValue("Content Type", mimeType, RegistryValueKind.String);
                    changed = true;
                }

                if ((percievedType != PercievedFileType.None) && ((string)extensionKey.GetValue("PerceivedType") != percievedType.ToString("g")))
                {
                    extensionKey.SetValue("PerceivedType", percievedType.ToString("g"), RegistryValueKind.String);
                    changed = true;
                }
            }

            if (changed) Shell32.ChangeNotify(ChangeNotifyEventId.AssociationChanged, ChangeNotifyFlags.IdList);
        }
    }
}
