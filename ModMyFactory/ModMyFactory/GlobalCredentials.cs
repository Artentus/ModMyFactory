using System;
using System.ComponentModel;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using ModMyFactory.Helpers;
using ModMyFactory.Views;
using ModMyFactory.Web;
using ModMyFactory.Web.AuthenticationApi;
using Newtonsoft.Json;
using WPFCore;

namespace ModMyFactory
{
    sealed class GlobalCredentials : NotifyPropertyChangedBase
    {
        [JsonObject(MemberSerialization.OptOut)]
        private struct CredentialsExportTemplate
        {
            public string Entropy;
            public string ProtectedUsername;
            public string ProtectedPassword;
        }


        public static GlobalCredentials Instance { get; }

        private static FileInfo CredentialsFile { get; }

        static GlobalCredentials()
        {
            CredentialsFile = new FileInfo(Path.Combine(App.Instance.AppDataPath, "credentials.json"));
            Instance = new GlobalCredentials();
        }

        private static byte[] GenerateEntropy()
        {
            byte[] entropy = new byte[64];

            var cryptoServiceProvider = new RNGCryptoServiceProvider();
            cryptoServiceProvider.GetBytes(entropy);

            return entropy;
        }


        string username;
        SecureString password;
        string token;

        public string Username
        {
            get { return username; }
            set
            {
                if (value != username)
                {
                    username = value;
                    token = null;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Username)));
                }
            }
        }

        public SecureString Password
        {
            get { return password; }
            set
            {
                password = value;
                token = null;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Password)));
            }
        }

        private void Load(FileInfo file)
        {
            CredentialsExportTemplate template = JsonHelper.Deserialize<CredentialsExportTemplate>(file);

            byte[] entropy = Convert.FromBase64String(template.Entropy);
            byte[] protectedUsernameBytes = Convert.FromBase64String(template.ProtectedUsername);
            byte[] protectedPasswordBytes = Convert.FromBase64String(template.ProtectedPassword);

            byte[] usernameBytes = ProtectedData.Unprotect(protectedUsernameBytes, entropy, DataProtectionScope.CurrentUser);
            byte[] passwordBytes = ProtectedData.Unprotect(protectedPasswordBytes, entropy, DataProtectionScope.CurrentUser);

            try
            {
                username = Encoding.Unicode.GetString(usernameBytes);
                password = SecureStringHelper.SecureStringFromBytes(passwordBytes, Encoding.Unicode);
            }
            finally
            {
                SecureStringHelper.DestroySecureByteArray(passwordBytes);
            }
        }

        private GlobalCredentials()
        {
            token = null;

            if (App.Instance.Settings.SaveCredentials)
            {
                try
                {
                    if (CredentialsFile.Exists) Load(CredentialsFile);
                }
                catch (CryptographicException)
                {
                    DeleteSave();
                }
            }
            else
            {
                DeleteSave();
            }
        }

        private bool IsLoggedIn() => !string.IsNullOrEmpty(Username) && (Password != null);

        private bool IsLoggedInWithToken() => IsLoggedIn() && !string.IsNullOrEmpty(token);

        public bool LogIn(Window owner, out string loginToken)
        {
            loginToken = null;

            bool failed = false;
            if (IsLoggedInWithToken()) // Credentials and token available.
            {
                // Token only expires on user request, best to not check and save bandwidth.
            }
            else if (IsLoggedIn()) // Only credentials available.
            {
                AuthenticationInfo info;
                failed = !ApiAuthentication.LogIn(Username, Password, out info);
                if (!failed)
                {
                    username = info.Username;
                    token = info.Token;
                    if (App.Instance.Settings.SaveCredentials) Save();
                }
            }

            if (failed)
            {
                token = null;
            }

            while (!IsLoggedInWithToken())
            {
                var loginWindow = new LoginWindow
                {
                    Owner = owner,
                    SaveCredentialsBox = { IsChecked = App.Instance.Settings.SaveCredentials },
                    FailedText = { Visibility = failed ? Visibility.Visible : Visibility.Collapsed },
                };
                bool? loginResult = loginWindow.ShowDialog();
                if (loginResult == null || loginResult == false) return false;
                username = loginWindow.UsernameBox.Text;
                password = loginWindow.PasswordBox.SecurePassword;

                bool saveCredentials = loginWindow.SaveCredentialsBox.IsChecked ?? false;
                App.Instance.Settings.SaveCredentials = saveCredentials;

                AuthenticationInfo info;
                failed = !ApiAuthentication.LogIn(Username, Password, out info);
                if (failed)
                {
                    token = null;
                }
                else
                {
                    username = info.Username;
                    token = info.Token;
                    if (saveCredentials) Save();
                }
            }

            loginToken = token;
            return true;
        }

        private void Save(FileInfo file)
        {
            byte[] usernameBytes = Encoding.Unicode.GetBytes(Username);
            byte[] passwordBytes = new byte[SecureStringHelper.GetSecureStringByteCount(Password, Encoding.Unicode)];
            SecureStringHelper.SecureStringToBytes(Password, passwordBytes, 0, Encoding.Unicode);

            try
            {
                byte[] entropy = GenerateEntropy();
                byte[] protectedUsernameBytes = ProtectedData.Protect(usernameBytes, entropy, DataProtectionScope.CurrentUser);
                byte[] protectedPasswordBytes = ProtectedData.Protect(passwordBytes, entropy, DataProtectionScope.CurrentUser);

                var template = new CredentialsExportTemplate()
                {
                    Entropy = Convert.ToBase64String(entropy),
                    ProtectedUsername = Convert.ToBase64String(protectedUsernameBytes),
                    ProtectedPassword = Convert.ToBase64String(protectedPasswordBytes),
                };

                JsonHelper.Serialize(template, file);
            }
            finally
            {
                SecureStringHelper.DestroySecureByteArray(passwordBytes);
            }
        }

        public void Save()
        {
            Save(CredentialsFile);
        }

        public void DeleteSave()
        {
            if (CredentialsFile.Exists) CredentialsFile.Delete();
        }
    }
}
