using GroupDocs.Conversion;
using GroupDocs.Conversion.FileTypes;
using GroupDocs.Conversion.Options.Convert;
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

                using var incomingWatcher = new FileSystemWatcher(@"C:\TestFolder\sftp_client1\incoming");
                incomingWatcher.EnableRaisingEvents = true;
                incomingWatcher.NotifyFilter = NotifyFilters.CreationTime |
                                       NotifyFilters.FileName |
                                       NotifyFilters.Size;
                incomingWatcher.Created += OnCreated;
                //incomingWatcher.Filter = "*.xlsx";

                using var stagingWatcher = new FileSystemWatcher(@"C:\TestFolder\sftp_client1\staging");
                stagingWatcher.EnableRaisingEvents = true;
                stagingWatcher.NotifyFilter = NotifyFilters.CreationTime |
                                       NotifyFilters.FileName |
                                       NotifyFilters.Size;
                stagingWatcher.Created += ConvertToCsv;
                //stagingWatcher.Filter = "*.xlsx";

                await Task.Delay(5000, stoppingToken);
            }
        }


        private static void OnCreated(object sender, FileSystemEventArgs e)
        {          
            //_logger.LogInformation("File " + e.Name + " created at: " + DateTime.Now.ToString());          
            //var fileName = e.Name;
            var sourcePath = @"C:\TestFolder\sftp_client1\incoming";
            var targetPath = @"C:\TestFolder\sftp_client1\staging";

            if (Directory.Exists(targetPath))
            {
                string[] files = Directory.GetFiles(sourcePath);                

                foreach (string s in files)
                {
                    var date = DateTime.Today.ToString("dd-MM-yyyy");
                    var fileName = $"to_send_to_ftp_{date}.xlsx" ;
                    var sendFile = Path.Combine(targetPath, fileName);
                    //_logger.LogInformation("File " + sendFileName + " has been moved to: " + targetPath);
                    
                    File.Move(s, sendFile, true);
                }
            }          
        }

        private static void ConvertToCsv(object sender, FileSystemEventArgs e)
        {
            var date = DateTime.Today.ToString("dd-MM-yyyy");
            var sourcePath = @"C:\TestFolder\sftp_client1\staging";
            var targetPath = @$"C:\TestFolder\sftp_client1\to_send\converted_file_{date}.csv";
            var copyFilePath = @"C:\TestFolder\sftp_client1\sent";

            string[] files = Directory.GetFiles(sourcePath);

            foreach(string s in files)
            {
                using (Converter converter = new Converter(s))
                {
                    SpreadsheetConvertOptions options = new SpreadsheetConvertOptions
                    {
                        PageNumber = 1,
                        PagesCount = 1,
                        Format = SpreadsheetFileType.Csv // Specify the conversion format
                    };

                    converter.Convert(targetPath, options);
                }

                
                var fileName = $"sent_{date}.csv";
                var copyFile = Path.Combine(copyFilePath, fileName);
                //_logger.LogInformation("File " + sendFileName + " has been moved to: " + targetPath);

                File.Copy(s, copyFile, true);
                File.Move(s, targetPath, true);                
            }
        }     
    }
}