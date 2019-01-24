using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toqe.Downloader.Business.Contract;
using Toqe.Downloader.Business.Contract.Events;
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

        static IProgress<double> progress2 = null;
        static DownloadProgressMonitor progressMonitor = null;
        static MultiPartDownload download = null;
        /// <summary>
        /// Downloads the response of an HTTP request and saves it as a file.
        /// </summary>
        /// <param name="url">The URL of the request.</param>
        /// <param name="container">The cookie container used to store cookies in the connection.</param>
        /// <param name="file">The file the data is written to.</param>
        /// <param name="progress">A progress object used to report the progress of the operation.</param>
        /// <param name="cancellationToken">A cancelation token that can be used to cancel the operation.</param>
        /// 

        public static async Task DownloadFileAsync(Uri url, CookieContainer container, FileInfo file, IProgress<double> progress, CancellationToken cancellationToken)
        {
      
            var requestBuilder = new SimpleWebRequestBuilder();
            var dlChecker = new DownloadChecker();

            var httpDlBuilder = new SimpleDownloadBuilder(requestBuilder, dlChecker);
            var timeForHeartbeat = 3000;
            var timeToRetry = 5000;
            var maxRetries = 5;
            var rdlBuilder = new ResumingDownloadBuilder(timeForHeartbeat, timeToRetry, maxRetries, httpDlBuilder);
            List<DownloadRange> alreadyDownloadedRanges = null;
            var bufferSize = 4096;
            var numberOfParts = 10;
            download = new MultiPartDownload(url, bufferSize, numberOfParts, rdlBuilder, requestBuilder, dlChecker, alreadyDownloadedRanges);
            var speedMonitor = new DownloadSpeedMonitor(maxSampleCount: 128);
            speedMonitor.Attach(download);
            progressMonitor = new DownloadProgressMonitor();
            progressMonitor.Attach(download);
            var dlSaver = new DownloadToFileSaver(file);
            dlSaver.Attach(download);
            download.DownloadCompleted += ModWebsite.OnCompleted;
            download.DataReceived += Download_DataReceived;
            progress2 = progress;
            await Task.Run(() => download.Start());
        }

        private static void Download_DataReceived(DownloadDataReceivedEventArgs args)
        {
            progress2.Report(progressMonitor.GetCurrentProgressPercentage(download));
        }
    }
}
