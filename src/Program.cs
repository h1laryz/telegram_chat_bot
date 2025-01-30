using System;
using Serilog;
using TelegramBot.Services;

class Program
{
    static async Task Main()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        string token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
        if (string.IsNullOrEmpty(token))
        {
            Log.Error("No token found. Exiting...");
            return;
        }

        var botService = new BotService(token);
        await botService.StartAsync();

        await Task.Delay(Timeout.Infinite);
    }
}