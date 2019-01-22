using System.Net;
using System.Security;
using System.Text;
using ModMyFactory.Helpers;
using ModMyFactory.Web.AuthenticationApi;

namespace ModMyFactory.Web
{
    static class ApiAuthentication
    {
        /// <summary>
        /// Logs in at the api.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="token">The users token.</param>
        public static AuthenticationInfo LogIn(string username, string token)
        {
            AuthenticationInfo info = new AuthenticationInfo();
            info.Username = username;
            info.Token = token;
        
            return info;
        }
    }
}
