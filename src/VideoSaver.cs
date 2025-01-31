using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;

public static class VideoSaver
{
    public static async Task SaveFromTiktokAsync(ITelegramBotClient botClient, Message message)
    {
        if (!string.IsNullOrEmpty(message.Text) && message.Text.Contains("tiktok.com", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string reqVideoUrl = $"https://www.tikwm.com/api/?url={message.Text}&hd=1";
                    HttpResponseMessage response = await client.GetAsync(reqVideoUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string body = await response.Content.ReadAsStringAsync();

                        try
                        {
                            JsonDocument json = JsonDocument.Parse(body);
                            if (json.RootElement.TryGetProperty("data", out JsonElement dataElement))
                            {
                                if (dataElement.TryGetProperty("hdplay", out JsonElement hdplayElement))
                                {
                                    string hdplayUrl = hdplayElement.GetString();
                                    await Task.Delay(500);

                                    await botClient.SendVideo(message.Chat.Id, hdplayUrl, replyParameters: message);
                                }
                            }
                        }
                        catch (JsonException ex)
                        {
                            Log.Error($"Ошибка разбора JSON: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Ошибка разбора JSON: {ex.Message}");
            }
        }
    }
}
