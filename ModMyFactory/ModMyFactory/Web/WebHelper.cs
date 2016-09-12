using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModMyFactory.Web
{
    /// <summary>
    /// Contains helper methods for web requests.
    /// </summary>
    static class WebHelper
    {
        static readonly string UserAgent;

        static WebHelper()
        {
            string osVersion = Environment.OSVersion.Version.ToString(2);
            string osPlatform = Environment.Is64BitOperatingSystem ? "Win64" : "Win32";
            string appPlatform = Environment.Is64BitProcess ? "x64" : "x86";
            UserAgent = $"Mozilla/5.0 (Windows NT {osVersion}; {osPlatform}; {appPlatform}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.101 Safari/537.36";
        }

        /// <summary>
        /// Creates a HTTP request using the GET method.
        /// </summary>
        /// <param name="url">The URL of the request.</param>
        /// <param name="container">The cookie container used to store cookies in the connection.</param>
        /// <returns>Returns a HTTPWebRequest object with its attributes set accordingly.</returns>
        public static HttpWebRequest CreateHttpRequest(string url, CookieContainer container)
        {
            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.CookieContainer = container;
            request.Method = WebRequestMethods.Http.Get;
            request.KeepAlive = true;
            request.UserAgent = UserAgent;
            return request;
        }

        /// <summary>
        /// Creates a HTTP request using the POST method.
        /// </summary>
        /// <param name="url">The URL of the request.</param>
        /// <param name="container">The cookie container used to store cookies in the connection.</param>
        /// <param name="content">The content that gets sent to the server.</param>
        /// <returns>Returns a HTTPWebRequest object with its attributes set accordingly.</returns>
        public static HttpWebRequest CreateHttpRequest(string url, CookieContainer container, string content)
        {
            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.CookieContainer = container;
            request.Method = WebRequestMethods.Http.Post;
            request.KeepAlive = true;
            request.UserAgent = UserAgent;
            request.ContentType = "application/x-www-form-urlencoded";

            byte[] contentData = Encoding.UTF8.GetBytes(content);
            request.ContentLength = contentData.Length;
            using (var stream = request.GetRequestStream())
                stream.Write(contentData, 0, contentData.Length);

            return request;
        }

        /// <summary>
        /// Gets the response document of an HTTP request.
        /// </summary>
        /// <param name="url">The URL of the request.</param>
        /// <param name="container">The cookie container used to store cookies in the connection.</param>
        /// <param name="document">Out. The received document.</param>
        /// <returns>Return false if the request failed, otherwise true.</returns>
        public static bool TryGetDocument(string url, CookieContainer container, out string document)
        {
            HttpWebRequest request = CreateHttpRequest(url, container);
            WebResponse response;
            try
            {
                response = request.GetResponse();
            }
            catch (WebException)
            {
                document = null;
                return false;
            }
            try
            {
                using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    document = reader.ReadToEnd();
            }
            catch (ProtocolViolationException)
            {
                document = null;
                return false;
            }
            finally
            {
                response.Close();
            }

            return true;
        }

        /// <summary>
        /// Gets the response document of an HTTP request.
        /// </summary>
        /// <param name="url">The URL of the request.</param>
        /// <param name="container">The cookie container used to store cookies in the connection.</param>
        /// <param name="content">The content that gets sent to the server.</param>
        /// <param name="document">Out. The received document.</param>
        /// <returns>Return false if the request failed, otherwise true.</returns>
        public static bool TryGetDocument(string url, CookieContainer container, string content, out string document)
        {
            HttpWebRequest request = CreateHttpRequest(url, container, content);
            WebResponse response;
            try
            {
                response = request.GetResponse();
            }
            catch (WebException)
            {
                document = null;
                return false;
            }
            try
            {
                using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    document = reader.ReadToEnd();
            }
            catch (ProtocolViolationException)
            {
                document = null;
                return false;
            }
            finally
            {
                response.Close();
            }

            return true;
        }

        /// <summary>
        /// Downloads the answer of an HTTP request and saves it as a file.
        /// </summary>
        /// <param name="url">The URL of the request.</param>
        /// <param name="container">The cookie container used to store cookies in the connection.</param>
        /// <param name="file">The file the data is written to.</param>
        /// <param name="progress">A progress object used to report the progress of the operation.</param>
        /// <param name="cancellationToken">A cancelation token that can be used to cancel the operation.</param>
        public static async Task DownloadFileAsync(Uri url, CookieContainer container, FileInfo file, IProgress<double> progress, CancellationToken cancellationToken)
        {
            try
            {
                var handler = new HttpClientHandler { CookieContainer = container, UseCookies = true };
                using (var client = new HttpClient(handler, true))
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
                    HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                    response.EnsureSuccessStatusCode();

                    using (Stream data = await response.Content.ReadAsStreamAsync())
                    {
                        long fileSize = response.Content.Headers.ContentLength.Value;
                        using (Stream fs = file.OpenWrite())
                        {
                            byte[] buffer = new byte[81920];
                            do
                            {
                                int byteCount = await data.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                                await fs.WriteAsync(buffer, 0, byteCount, cancellationToken);

                                progress.Report((double)fs.Length / fileSize);
                            } while (fs.Length < fileSize);
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                if (file.Exists) file.Delete();
            }
        }
    }
}
