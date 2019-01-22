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
            public string ProtectedToken;
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
        string token;

        public string Username
        {
            get { return username; }
            set
            {
                if (value != username)
                {
                    username = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Username)));
                }
            }
        }

        public string Token
        {
            get { return token; }
            set
            {
                token = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Token)));
            }
        }

        private void Load(FileInfo file)
        {
            CredentialsExportTemplate template = JsonHelper.Deserialize<CredentialsExportTemplate>(file);

            byte[] entropy = Convert.FromBase64String(template.Entropy);
            byte[] protectedUsernameBytes = Convert.FromBase64String(template.ProtectedUsername);
            byte[] protectedTokenBytes = Convert.FromBase64String(template.ProtectedToken);

            byte[] usernameBytes = ProtectedData.Unprotect(protectedUsernameBytes, entropy, DataProtectionScope.CurrentUser);
            byte[] tokenBytes = ProtectedData.Unprotect(protectedTokenBytes, entropy, DataProtectionScope.CurrentUser);

            try
            {
                username = Encoding.Unicode.GetString(usernameBytes);
                token = Encoding.Unicode.GetString(tokenBytes);
            }
            finally
            {
                SecureStringHelper.DestroySecureByteArray(tokenBytes);
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

        private bool IsLoggedIn() => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Token);

        private bool IsLoggedInWithToken() => IsLoggedIn() && !string.IsNullOrEmpty(token);

        public bool LogIn(Window owner, out string loginToken)
        {

            loginToken = null;

            if (IsLoggedInWithToken()) // Credentials and token available.
            {
                // Token only expires on user request, best to not check and save bandwidth.
            }
            else if (IsLoggedIn()) // Only credentials available.
            {
                AuthenticationInfo info = ApiAuthentication.LogIn(Username, Token);
                username = info.Username;
                token = info.Token;
                if (App.Instance.Settings.SaveCredentials) Save();
            }


            while (!IsLoggedInWithToken())
            {
                var loginWindow = new LoginWindow
                {
                    Owner = owner,
                    SaveCredentialsBox = { IsChecked = App.Instance.Settings.SaveCredentials },
                };
                bool? loginResult = loginWindow.ShowDialog();
                if (loginResult == null || loginResult == false) return false;
                username = loginWindow.UsernameBox.Text;
                token = loginWindow.TokenBox.Text;
                bool saveCredentials = loginWindow.SaveCredentialsBox.IsChecked ?? false;
                App.Instance.Settings.SaveCredentials = saveCredentials;

                AuthenticationInfo info = ApiAuthentication.LogIn(Username, Token);
                username = info.Username;
                token = info.Token;
                if (saveCredentials) Save();
            }

            loginToken = token;
            return true;
        }

        private void Save(FileInfo file)
        {
            byte[] usernameBytes = Encoding.Unicode.GetBytes(Username);
            byte[] tokenBytes = Encoding.Unicode.GetBytes(Token);

            try
            {
                byte[] entropy = GenerateEntropy();
                byte[] protectedUsernameBytes = ProtectedData.Protect(usernameBytes, entropy, DataProtectionScope.CurrentUser);
                byte[] protectedTokenBytes = ProtectedData.Protect(tokenBytes, entropy, DataProtectionScope.CurrentUser);

                var template = new CredentialsExportTemplate()
                {
                    Entropy = Convert.ToBase64String(entropy),
                    ProtectedUsername = Convert.ToBase64String(protectedUsernameBytes),
                    ProtectedToken = Convert.ToBase64String(protectedTokenBytes),
                };

                JsonHelper.Serialize(template, file);
            }
            finally
            {
                SecureStringHelper.DestroySecureByteArray(tokenBytes);
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
