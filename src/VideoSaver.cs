using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Net.Http.Headers;

public static class VideoSaver
{
    static readonly string API_KEY = Environment.GetEnvironmentVariable("TIKTOK_API_KEY");

    public static async Task SaveFromTiktokAsync(ITelegramBotClient botClient, Message message)
    {
        if (!string.IsNullOrEmpty(message.Text) && message.Text.Contains("tiktok.com", StringComparison.OrdinalIgnoreCase))
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://tiktok-api23.p.rapidapi.com/api/download/video?url=" + message.Text),
                Headers =
                {
                    { "X-RapidAPI-Key", API_KEY },
                    { "X-RapidAPI-Host", "tiktok-api23.p.rapidapi.com" },
                },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();

                var body = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(body);

                if (json.RootElement.TryGetProperty("data", out var dataElement))
                {
                    if (dataElement.TryGetProperty("play", out var playElement))
                    {
                        var playUrl = playElement.GetString();

                        await botClient.SendVideo(message.Chat.Id, playUrl, replyParameters: message);
                    }
                }
            }
        }
    }
}
