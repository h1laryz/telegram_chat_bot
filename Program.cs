using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

class Program
{
    private static readonly string BotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");

    static async Task Main(string[] args)
    {
        var botClient = new TelegramBotClient(BotToken);

        botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync);

        Console.WriteLine("Bot is running... Press any key to exit.");
        Console.ReadKey();
    }

    static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type != UpdateType.Message || update.Message == null)
            return;

        var message = update.Message;

        if (message.Chat.Type != ChatType.Group && message.Chat.Type != ChatType.Supergroup)
            return;

        if (message.Text != null 
            && (message.Text.Contains("κᴀппеp") 
            || message.Text.Contains("пpивᴀт")
            || message.Text.Contains("бecπлaτнo")
            || message.Text.Contains("слuвᴀю")
        ))
        {
            try
            {
                // Ban the user
                await botClient.BanChatMember(
                    chatId: message.Chat.Id,
                    userId: message.From.Id,
                    cancellationToken: cancellationToken
                );

                // Inform the group
                await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: $"Пользователь @{message.From.Username ?? Convert.ToString(message.From.Id) } был забанен за рекламу. Если это ошибка пиши сюда -> @h1lary",
                    cancellationToken: cancellationToken
                );

                // Delete the user's message
                await botClient.DeleteMessage(
                    chatId: message.Chat.Id,
                    messageId: message.MessageId,
                    cancellationToken: cancellationToken
                );

                Console.WriteLine($"User {message.From.Username ?? message.From.FirstName} banned in {message.Chat.Title}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error banning user: {ex.Message}");
            }
        }
    }

    static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Error: {exception.Message}");
        return Task.CompletedTask;
    }
}
