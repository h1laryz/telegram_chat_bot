using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using var cts = new CancellationTokenSource();

var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
if (token == null)
{
    Console.WriteLine("No token.");
    return;
}

var bot = new TelegramBotClient(token, cancellationToken: cts.Token);
var me = await bot.GetMe();
bot.OnError += OnError;
bot.OnMessage += OnMessage;
bot.OnUpdate += OnUpdate;

Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
Console.ReadLine();
cts.Cancel(); // stop the bot

// method to handle errors in polling or in your OnMessage/OnUpdate code
async Task OnError(Exception exception, HandleErrorSource source)
{
    Console.WriteLine(exception); // just dump the exception to the console
}

// method that handle messages received by the bot:
async Task OnMessage(Message message, UpdateType type)
{
    if (type != UpdateType.Message || message == null)
    {
        Console.WriteLine("update.Type != UpdateType.Message || update.Message == null");
        return;
    }
        

    if (message.Chat.Type != ChatType.Group && message.Chat.Type != ChatType.Supergroup)
    {
        Console.WriteLine("message.Chat.Type != ChatType.Group && message.Chat.Type != ChatType.Supergroup");
        return;
    }
            

    if (message.Text != null 
        && (message.Text.Contains("κᴀппеp") 
        || message.Text.Contains("пpивᴀт")
        || message.Text.Contains("бecπлaτнo")
        || message.Text.Contains("слuвᴀю")
        || (message.Text.Contains("сливаю") && message.Text.Contains("каппер"))
    ))
    {
        try
        {
            var chatMember = await bot.GetChatMember(
                chatId: message.Chat.Id,
                userId: message.From.Id,
                cancellationToken: cts.Token
            );

            // Check if the user is an administrator or the chat owner
            if (chatMember.Status == ChatMemberStatus.Administrator || chatMember.Status == ChatMemberStatus.Creator)
            {
                Console.WriteLine($"Skipped banning admin/owner: {message.From.Username ?? message.From.FirstName}");
                return;
            }

            // Ban the user
            await bot.BanChatMember(
                chatId: message.Chat.Id,
                userId: message.From.Id,
                cancellationToken: cts.Token
            );

            // Inform the group
            await bot.SendMessage(
                chatId: message.Chat.Id,
                text: $"Пользователь @{message.From.Username ?? Convert.ToString(message.From.Id) } был забанен за рекламу.\nЕсли это ошибка пиши сюда -> @h1lary",
                cancellationToken: cts.Token
            );

            // Delete the user's message
            await bot.DeleteMessage(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                cancellationToken: cts.Token
            );

            Console.WriteLine($"User {message.From.Username ?? message.From.FirstName} banned in {message.Chat.Title}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error banning user: {ex.Message}");
        }
    }
}

// method that handle other types of updates received by the bot:
async Task OnUpdate(Update update)
{
    
}
