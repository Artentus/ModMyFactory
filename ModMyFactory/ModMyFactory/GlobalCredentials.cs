using System.Security;

namespace ModMyFactory
{
    static class GlobalCredentials
    {
        public static bool LoggedIn { get; set; }

        public static string Username { get; set; }

        public static SecureString Password { get; set; }
    }
}
