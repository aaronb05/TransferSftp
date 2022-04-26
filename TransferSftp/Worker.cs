using System;
using System.IO;

namespace TransferSftp
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        //private List<string> filePaths = new List<string> { @"C:\TestFolder\sftp_client1\incoming", @"C:\TestFolder\sftp_client2\incoming" };

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        /// Need to add these 3 methods when creating windows service
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);

        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {

        }
        /// 

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                using var watcher = new FileSystemWatcher(@"C:\TestFolder\sftp_client1\incoming");
                watcher.EnableRaisingEvents = true;
                watcher.NotifyFilter = NotifyFilters.CreationTime |
                                       NotifyFilters.FileName |
                                       NotifyFilters.Size;
                watcher.Created += OnCreated;
                watcher.Filter = "*.txt";

                await Task.Delay(5000, stoppingToken);
            }
        }


        private static void OnCreated(object sender, FileSystemEventArgs e)
        {           
            //_logger.LogInformation("File " + e.Name + " created at: " + DateTime.Now.ToString());
          
            //var fileName = e.Name;
            var sourcePath = "C:\\TestFolder\\sftp_client1\\incoming";
            var targetPath = "C:\\TestFolder\\sftp_client1\\to_send";

            if (Directory.Exists(targetPath))
            {
                string[] files = Directory.GetFiles(sourcePath);                

                foreach (string s in files)
                {
                    var date = DateTime.Today.ToString("dd-MM-yyyy");
                    var sendFileName = "to_send_to_ftp" + date;
                    Console.WriteLine(date);
                    var destFile = Path.Combine(targetPath, sendFileName);
                    //_logger.LogInformation("File " + sendFileName + " has been moved to: " + targetPath);

                    File.Move(s, destFile, true);
                }
            }          
        }
    }
}