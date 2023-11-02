// relativity

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using HtmlAgilityPack;

// public class
public class Program
{
    private string httpsUrl = "FTP";
    private string httpsUsername = "USER";
    private string httpsPassword = "PASS";
    private string proxyHost = "PROXY"; 
    private int proxyPort = 8080;
    private string proxyUsername = "LaptopUsername"; // replace with username
    private string proxyPassword = "LaptopPassword"; // replace with password
    private string destinationFolder = @"G:\Bloomberg\DailyDiff";

    // builder class
    public class Builder
    {
        private string httpsUrl = string.Empty;
        private string httpsUsername = string.Empty;
        private string httpsPassword = string.Empty;
        private string proxyHost = string.Empty;
        private int proxyPort = 0;
        private string proxyUsername = string.Empty;
        private string proxyPassword = string.Empty;
        private string destinationFolder = string.Empty;

        public Builder WithHttpsUrl(string httpsUrl)
        {
            this.httpsUrl = httpsUrl;
            return this
        }
        public Builder WithDestinationFolder(string destinationFolder)
        {
            this.destinationFolder = destinationFolder;
            return this
        }

        public Builder WithProxy(string proxyHost, int proxyPort, string proxyUsername, string proxyPassword)
        {
            this.proxyHost = proxyHost;
            this.proxyPort = proxyPort;
            this.proxyUsername = proxyUsername;
            this.proxyPassword = proxyPassword;
            return this
        }

        public Program Build()
        {
            return new Program(httpsUrl, destinationFolder, proxyHost, proxyPort, proxyUsername, proxyPassword)
        }
    }

    private Program(
        string httpsUrl
        string httpsUsername
        string httpsPassword
        string destinationFolder
        string proxyHost
        int proxyPort
        string proxyUsername
        string proxyPassword)
    {
        this.httpsUrl = httpsUrl;
        this.httpsUsername = httpsUsername;
        this.httpsPassword = httpsPassword;
        this.destinationFolder = destinationFolder;
        this.proxyHost = proxyHost;
        this.proxyPort = proxyPort;
        this.proxyUsername = proxyUsername;
        this.proxyPassword = proxyPassword
    }

    public void CheckAndDownload()
    {
        while(true)
        {
            CheckAndDownloadNewFile();
            // TODO: cronjob? or any other easier way // schedule task
            Thread.Sleep(TimeSpan.FromHours(3)); // check https every 3 hours to download file
        }
    }

    private void CheckAndDownloadNewFile()
    {
        // https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-7.0
        // configure httpclient to use proxy server
        // proxy server with authentication
        var httpClientHandler = new HttpClientHandler
        {
            Proxy = new WebProxy(proxyHost, proxyPort),
            UseProxy = true,
            PreAuthenticate = true,
            UseDefaultCredentials = true
            // TODO: These should be set as creds to the network proxy 
            Credentials = new NetworkCredential(proxyUsername, proxyPassword)
        };
        // https://learn.microsoft.com/en-us/dotnet/api/system.net.networkcredential?view=net-7.0
        // configure credentials for https website
        var credentials = new NetworkCredential('USER', 'PASS');
        var credentialCache = new CredentialCache {{new Uri('FTP'), 'Basic', credentials }};
        httpClientHandler.Credentials = credentialCache;
        //TODO: These credentials need to be set in the handler constructor

        using (var httpClient = new HttpClient(httpClientHandler))

        try
        {   
            // file names are links on htmml
            // parse html content, extract links, filter files
            httpClient.BaseAddress = new Uri('FTP');
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue('text/html'));

            var response = httpClient.GetAsync('FTP').Result;
            // https://stackoverflow.com/questions/32569860/checking-if-httpstatuscode-represents-success-or-failure
            if (response.IsSuccessStatusCode)
            {
                var htmlContent = response.Content.ReadAsStringAsync().Result;
                var fileNames = GetFileNamesFromHtml(htmlContent);

                foreach (var fileName in fileNames)
                {
                    if (fileName.EndsWith('.gpg'))
                    {
                        DownloadAndDecryptFile(fileName);
                    }
                }
            }
            else
            {
                Console.WriteLine($'No New File Uploaded: {response.ReasonPhrase}');
            }
            
        }
        catch (Exception ex)
        {
            // TODO: Output the entire stacktrace (ex) not just the top level message
            Console.WriteLine($'No New File Uploaded: {ex.Message}');
        }
    }
    static void DownloadAndDecryptFile(string filename)
}
