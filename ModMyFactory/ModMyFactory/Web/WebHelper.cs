using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toqe.Downloader.Business.Contract;
using Toqe.Downloader.Business.Download;
using Toqe.Downloader.Business.DownloadBuilder;
using Toqe.Downloader.Business.Observer;
using Toqe.Downloader.Business.Utils;

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
        public static HttpWebRequest CreateHttpRequest(string url, CookieContainer container, byte[] content)
        {
            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.CookieContainer = container;
            request.Method = WebRequestMethods.Http.Post;
            request.KeepAlive = true;
            request.UserAgent = UserAgent;
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
        /// <param name="container">The cookie container used to store cookies in the connection.</param>
        /// <returns>Returns the received document.</returns>
        public static string GetDocument(string url, CookieContainer container)
        {
            string document = string.Empty;
            HttpWebRequest request = CreateHttpRequest(url, container);

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
        /// <param name="container">The cookie container used to store cookies in the connection.</param>
        /// <param name="content">The content that gets sent to the server.</param>
        /// <returns>Returns the received document.</returns>
        public static string GetDocument(string url, CookieContainer container, byte[] content)
        {
            string document = string.Empty;
            HttpWebRequest request = CreateHttpRequest(url, container, content);

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
        /// <param name="container">The cookie container used to store cookies in the connection.</param>
        /// <param name="file">The file the data is written to.</param>
        /// <param name="progress">A progress object used to report the progress of the operation.</param>
        /// <param name="cancellationToken">A cancelation token that can be used to cancel the operation.</param>
        public static async Task DownloadFileAsync(Uri url, CookieContainer container, FileInfo file, IProgress<double> progress, CancellationToken cancellationToken)
        {
            try
            {
                if (ServicePointManager.DefaultConnectionLimit <= 4)
                {
                    ServicePointManager.DefaultConnectionLimit = 5;
                }
                var requestBuilder = new SimpleWebRequestBuilder(null);
                var dlChecker = new DownloadChecker();
                var rdlBuilder = new ResumingDownloadBuilder(3000, 5000, 5, new SimpleDownloadBuilder(requestBuilder, dlChecker));
                System.Collections.Generic.List<DownloadRange> range = null;
                var download = new MultiPartDownload(url, 65536,3, rdlBuilder, requestBuilder, dlChecker, range);

                SemaphoreSlim signal = new SemaphoreSlim(0, 1);
                bool Complete = false;

                double getDownloadPercent(MultiPartDownload downloads)
                {

                    long downloaded = 0;
                    long filesize = 0;
                    foreach (var a in downloads.AlreadyDownloadedRanges)
                    {
                        downloaded += a.End - a.Start;
                        filesize = (a.End > filesize) ? a.End : filesize;
                    }
                    foreach (var a in downloads.ToDoRanges)
                    {
                        filesize = (a.End > filesize) ? a.End : filesize;
                    }
                    filesize = filesize + 1;
                    return Convert.ToDouble( downloaded) / Convert.ToDouble(filesize);
                }
                void Download_DataReceived(Toqe.Downloader.Business.Contract.Events.DownloadDataReceivedEventArgs args)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        download.Stop();
                        return;
                    }
                    
                    progress.Report(getDownloadPercent(download));
                }
                void Download_Completed(Toqe.Downloader.Business.Contract.Events.DownloadEventArgs args)
                {
                    Complete = true;
                }
                download.DataReceived += Download_DataReceived;
                download.DownloadCompleted += Download_Completed;

                var dlSaver = new DownloadToFileSaver(file);
                dlSaver.Attach(download);
                
                void threadedDownload()
                {
                    download.Start();

                    ManualResetEventSlim mres = new ManualResetEventSlim(false);
                    Random RND = new Random();
                    for (int wait=0;wait< RND.Next(55,80);wait++)
                    {
                        mres.Wait(1000);
                        if(Complete)
                        {
                            signal.Release();
                            return;
                        }
                    }
                    while (true)
                    {
                        
                        range = download.AlreadyDownloadedRanges;
                        download.Stop();
                        download = new MultiPartDownload(url, 65536, 3, rdlBuilder, requestBuilder, dlChecker, range);

                        download.DataReceived += Download_DataReceived;
                        download.DownloadCompleted += Download_Completed;

                        dlSaver.Attach(download);
                        download.Start();
                        for (int wait = 0; wait < RND.Next(40, 80); wait++)
                        {
                            mres.Wait(1000);
                            if (Complete || download.ToDoRanges.Count == 0)
                            {
                                download.Stop();
                                signal.Release();
                                return;
                            }
                        }
                    }
                }
                Thread th = new Thread(new ThreadStart(threadedDownload));
                
                th.Start();

                await signal.WaitAsync();
                dlSaver.DetachAll();

                Complete = true;
                if (th.ThreadState == ThreadState.Running)
                {
                    th.Abort();
                }

                return;
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

            try
            {
                var handler = new HttpClientHandler();
                if (container != null)
                {
                    handler.CookieContainer = container;
                    handler.UseCookies = true;
                }

                using (var client = new HttpClient(handler, true))
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
