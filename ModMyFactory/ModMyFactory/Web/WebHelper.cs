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
        const string UserAgent = "ModMyFactory";

        /// <summary>
        /// Creates a HTTP request using the GET method.
        /// </summary>
        /// <param name="url">The URL of the request.</param>
        /// <returns>Returns a HTTPWebRequest object with its attributes set accordingly.</returns>
        public static HttpWebRequest CreateHttpRequest(string url)
        {
            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Method = WebRequestMethods.Http.Get;
            request.KeepAlive = true;
            request.UserAgent = UserAgent;
            request.Proxy = null;
            return request;
        }

        /// <summary>
        /// Creates a HTTP request using the POST method.
        /// </summary>
        /// <param name="url">The URL of the request.</param>
        /// <param name="content">The content that gets sent to the server.</param>
        /// <returns>Returns a HTTPWebRequest object with its attributes set accordingly.</returns>
        public static HttpWebRequest CreateHttpRequest(string url, byte[] content)
        {
            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Method = WebRequestMethods.Http.Post;
            request.KeepAlive = true;
            request.UserAgent = UserAgent;
            request.Proxy = null;
            request.ContentType = "application/x-www-form-urlencoded";

            request.ContentLength = content.Length;
            using (var stream = request.GetRequestStream())
                stream.Write(content, 0, content.Length);

            return request;
        }

        /// <summary>
        /// Gets the response document of an HTTP request.
        /// </summary>
        /// <param name="url">The URL of the request.</param>
        /// <returns>Returns the received document.</returns>
        public static string GetDocument(string url)
        {
            string document = string.Empty;
            HttpWebRequest request = CreateHttpRequest(url);

            WebResponse response = null;
            Stream responseStream = null;
            try
            {
                response = request.GetResponse();
                responseStream = response.GetResponseStream();

                if (responseStream != null)
                {
                    using (var reader = new StreamReader(responseStream, Encoding.UTF8))
                        document = reader.ReadToEnd();
                }
            }
            finally
            {
                responseStream?.Close();
                response?.Close();
            }
            
            return document;
        }

        /// <summary>
        /// Gets the response document of an HTTP request.
        /// </summary>
        /// <param name="url">The URL of the request.</param>
        /// <param name="content">The content that gets sent to the server.</param>
        /// <returns>Returns the received document.</returns>
        public static string GetDocument(string url, byte[] content)
        {
            string document = string.Empty;
            HttpWebRequest request = CreateHttpRequest(url, content);

            WebResponse response = null;
            Stream responseStream = null;
            try
            {
                response = request.GetResponse();
                responseStream = response.GetResponseStream();

                if (responseStream != null)
                {
                    using (var reader = new StreamReader(responseStream, Encoding.UTF8))
                        document = reader.ReadToEnd();
                }
            }
            finally
            {
                responseStream?.Close();
                response?.Close();
            }

            return document;
        }

        /// <summary>
        /// Downloads the response of an HTTP request and saves it as a file.
        /// </summary>
        /// <param name="url">The URL of the request.</param>
        /// <param name="file">The file the data is written to.</param>
        /// <param name="progress">A progress object used to report the progress of the operation.</param>
        /// <param name="cancellationToken">A cancelation token that can be used to cancel the operation.</param>
        public static async Task DownloadFileAsync(Uri url, FileInfo file, IProgress<double> progress, CancellationToken cancellationToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);

                    HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                    response.EnsureSuccessStatusCode();

                    using (Stream data = await response.Content.ReadAsStreamAsync())
                    {
                        long? fileSize = response.Content.Headers.ContentLength;

                        if (file.Directory?.Exists == false) file.Directory.Create();
                        using (Stream fs = file.OpenWrite())
                        {
                            byte[] buffer = new byte[8192];
                            int byteCount;
                            do
                            {
                                byteCount = await data.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                                if (byteCount > 0) await fs.WriteAsync(buffer, 0, byteCount, cancellationToken);

                                if (fileSize.HasValue) progress.Report((double)fs.Length / fileSize.Value);
                            } while (byteCount > 0);
                        }
                    }

                    progress.Report(1);
                }
            }
            catch (TaskCanceledException)
            {
                if (file.Exists) file.Delete();
            }
            catch (Exception)
            {
                if (file.Exists) file.Delete();
                throw;
            }
        }
    }
}
