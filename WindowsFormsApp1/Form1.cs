using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.IO;

namespace WindowsFormsApp1
{
 
    public partial class Launcher_Updater : Form
    {

        private const string URL = "http://127.0.0.1:5000/";


        public Launcher_Updater()
        {
            InitializeComponent();

            var progress = new Progress<int>(value => { downloadBar.Value = value; });
            Task.Run(()=> DownloadLastLauncherVersion(progress));
        }

        static async Task DownloadLastLauncherVersion(IProgress<int> progress)
        {
            var version = await GetVersionAsync().ConfigureAwait(false);

            //Download launcher binary
            var downloadPath = Path.Combine(Directory.GetCurrentDirectory(), "Mu Patrulla.exe");
            
            using (var client = new HttpClientDownloadWithProgress(version.uri, downloadPath))
            {
                client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) => {

                    progress.Report((int)progressPercentage);
                };

                await client.StartDownload();
            }

        }

        static async Task<LauncherUpdate> GetVersionAsync()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            try
            {
                LauncherUpdate version = null;
                HttpResponseMessage response = await client.GetAsync("launcher/last-version").ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    var version_json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    version = JsonSerializer.Deserialize<LauncherUpdate>(version_json);
                }

                return version;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

        }
    }

    public class LauncherUpdate
    {
        public string status { get; set; }
        public int version_number { get; set; }
        public string uri { get; set; }
        public string message { get; set; }
    }

}
