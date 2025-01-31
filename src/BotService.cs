using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Serilog;

namespace TelegramBot.Services
{
    public class BotService
    {
        private readonly TelegramBotClient _bot;
        private readonly CancellationTokenSource _cts;
        private readonly MessageHandler _messageHandler;
        static public string Username {get; private set;} = ""; 

        public BotService(string token)
        {
            _bot = new TelegramBotClient(token);
            _cts = new CancellationTokenSource();
            _messageHandler = new MessageHandler(_bot, _cts.Token);
        }

        public async Task StartAsync()
        {
            Log.Information("Starting bot...");
            var me = await _bot.GetMe();
            Username = me.Username;
            Log.Information($"Bot @{me.Username} started.");

            _bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() },
                _cts.Token
            );
        }

        private async Task HandleUpdateAsync(ITelegramBotClient bot, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
        {
            await _messageHandler.HandleUpdateAsync(update);
        }

        private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
        {
            Log.Error(exception, "An error occurred in the bot");
            return Task.CompletedTask;
        }
    }
}