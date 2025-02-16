using System;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Text.Json;
using System.Net.Http;


class Program
{
    static async Task Main(string[] args)
    {
        var botClient = new TelegramBotClient("7754226406:AAEifOH8kYQsqf02-M5gwVAi3SyYUgE-L5o");
        using var cancelToken = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message }
        };

        botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancelToken.Token);

        var botInfo = await botClient.GetMe();
        Console.WriteLine($"Бот {botInfo.Username} запущен");

        await Task.Delay(-1);
    }

    static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancelToken)
    {
        if (update.Message is not { } message) return;

        //if (message.Text != null)
        //{
        //    await botClient.SendMessage(
        //    chatId: update.Message.Chat.Id,
        //    text: "Дурак?",
        //    cancellationToken: cancelToken);
        //    return;
        //}

        await SendVideoUrl(botClient, update, message, cancelToken);
        
    }

    static async Task SendVideoUrl(ITelegramBotClient botClient, Update update, Message message, CancellationToken cancelToken)
    {
        if (message.Text.Contains("tiktok.com") == false)
        {
            await botClient.SendMessage(
                chatId: update.Message.Chat.Id,
                text: "Это не tiktok.com",
                cancellationToken: cancelToken);
            return;
        }
        else
        {
            string videoUrl = await GetTikTokVideoUrl(message.Text);

            if (videoUrl == null)
            {
                await botClient.SendMessage(
                chatId: update.Message.Chat.Id,
                text: "Не удалось скачать видео. Попробуйте другую ссылку.",
                cancellationToken: cancelToken);
                return;
            }
            else
            {
                await botClient.SendVideo(
                chatId: update.Message.Chat.Id,
                video: videoUrl,
                cancellationToken: cancelToken);
            }
        }
    }

    static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancelToken)
    {
        Console.WriteLine($"Ошибка {exception.Message}");
        return Task.CompletedTask;
    }

    static async Task<string> GetTikTokVideoUrl(string url)
    {
        using var httpClient = new HttpClient();

        string apiUrl = $"https://www.tikwm.com/api/?url={url}";

        var responce = await httpClient.GetStringAsync(apiUrl);

        using var jsonDoc = JsonDocument.Parse(responce);
        var root = jsonDoc.RootElement;

        if (root.GetProperty("data").TryGetProperty("play", out var videoUrl))
        {
            //Console.WriteLine(videoUrl);
            return videoUrl.GetString();
        }


        return null;
    }
}