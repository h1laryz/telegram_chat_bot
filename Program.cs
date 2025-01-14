using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Serilog;
using System.Text.RegularExpressions;
using System.IO;

using var cts = new CancellationTokenSource();

var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
if (token == null)
{
    Console.WriteLine("No token.");
    return;
}

// Получаем текущую дату и время
string botStartTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

// Формируем имя файла с датой и временем
string fileName = $"{botStartTime}_telegram_bot.txt";

// Записываем в файл
File.WriteAllText(fileName, $"Bot started at: {botStartTime}\n");

// Configure Serilog for logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var bot = new TelegramBotClient(token, cancellationToken: cts.Token);
var me = await bot.GetMe();
bot.OnError += OnError;
bot.OnMessage += OnMessage;
bot.OnUpdate += OnUpdate;

Log.Information($"@{me.Username} is running... Press Enter to terminate");
Console.ReadLine();
cts.Cancel(); // stop the bot

bool IsMessageFromChannel(Message message)
{
    return message.From == null; // Если From == null, это канал
}

string CleanText(string text)
{
    // Удаление скобок ( и )
    text = text.Replace("(", "").Replace(")", "");

    // Регулярное выражение для удаления эмоджи
    var emojiPattern = new Regex(@"\p{So}|\p{Cs}\p{Cs}(\p{Cf}\p{Cs}\p{Cs})*");

    // Заменить только эмоджи
    text = emojiPattern.Replace(text, "");

    return text;
}

// Проверка текста
bool ShouldBan(string text)
{
    text = CleanText(text);
    Log.Information($"Проверка текста: \"{text}\"");

    if (string.IsNullOrWhiteSpace(text))
        return false;

    // Регулярное выражение для русского слова
    var russianWordPattern = new Regex(@"[а-яА-ЯёЁ]+");

    // Регулярное выражение для подозрительных символов (с учётом + как разрешённого символа)
    var suspiciousPattern = new Regex(@"[^а-яА-ЯёЁ0-9\s.,!?\""\-()&+]+");

    // Разбиваем текст на слова
    var words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
    
    int suspiciousWordsCount = 0;

    foreach (var word in words)
    {
        // Проверяем, если слово целиком русское
        if (russianWordPattern.IsMatch(word))
        {
            Log.Information($"{word} is russian.");
            // Проверяем, есть ли подозрительные символы в русском слове
            if (suspiciousPattern.IsMatch(word))
            {
                Log.Information($"{word} is suspicious.");
                ++suspiciousWordsCount;
            }
            else
            {
                Log.Information($"{word} is not suspicious.");
            }
        }
    }

    return suspiciousWordsCount >= 3;
}

bool ShouldDeleteMessage(string text)
{
    text = CleanText(text);

    var words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

    // Слова, которые нужно проверять
    var targetWords = new[] { "@", "t.me/" };
    int targetWordCount = words.Count(word => targetWords.Contains(word.ToLower()));

    return targetWordCount >= 1;
}

// Метод обработки ошибок
async Task OnError(Exception exception, HandleErrorSource source)
{
    Log.Error(exception, "An error occurred while processing the update");
}

// Метод обработки сообщений
async Task OnMessage(Message message, UpdateType type)
{
    if (type != UpdateType.Message || message == null)
    {
        Log.Information("update.Type != UpdateType.Message || update.Message == null");
        return;
    }

    if (message.Chat.Type != ChatType.Group && message.Chat.Type != ChatType.Supergroup)
    {
        Log.Information("message.Chat.Type != ChatType.Group && message.Chat.Type != ChatType.Supergroup");
        return;
    }

    // Проверка, если сообщение отправлено каналом
    if (IsMessageFromChannel(message))
    {
        Log.Information("Message is from a channel, skipping.");
        return;
    }

    // Check if the message has text
    if ((message.Text != null && ShouldBan(message.Text)) || (message.Caption != null && ShouldBan(message.Caption)))
    {
        try
        {
            var chatMember = await bot.GetChatMember(
                chatId: message.Chat.Id,
                userId: message.From.Id,
                cancellationToken: cts.Token
            );

            // Проверка на администратора или владельца
            if (chatMember.Status == ChatMemberStatus.Administrator || chatMember.Status == ChatMemberStatus.Creator)
            {
                Log.Information($"Skipped banning admin/owner: {message.From.Username ?? message.From.FirstName}");
                return;
            }

            // Бан пользователя
            await bot.BanChatMember(
                chatId: message.Chat.Id,
                userId: message.From.Id,
                cancellationToken: cts.Token
            );

            // Сообщение в группу
            await bot.SendMessage(
                chatId: message.Chat.Id,
                text: $"Пользователь @{message.From.Username ?? Convert.ToString(message.From.Id)} был забанен за подозрительное сообщение.\nЕсли это ошибка, пиши сюда -> @h1lary",
                cancellationToken: cts.Token
            );

            // Удаление сообщения
            await bot.DeleteMessage(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                cancellationToken: cts.Token
            );

            Log.Information($"User {message.From.Username ?? Convert.ToString(message.From.Id)} banned in {message.Chat.Title}.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error banning user");
        }
    }
    else if ((message.Text != null && ShouldDeleteMessage(message.Text)) || (message.Caption != null && ShouldDeleteMessage(message.Caption)))
    {
        try
        {
            var chatMember = await bot.GetChatMember(
                chatId: message.Chat.Id,
                userId: message.From.Id,
                cancellationToken: cts.Token
            );

            // Проверка на администратора или владельца
            if (chatMember.Status == ChatMemberStatus.Administrator || chatMember.Status == ChatMemberStatus.Creator)
            {
                Log.Information($"Skipped banning admin/owner: {message.From.Username ?? message.From.FirstName}");
                return;
            }

            // Удаление сообщения
            await bot.DeleteMessage(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                cancellationToken: cts.Token
            );

            Log.Information($"Message from {message.From.Username ?? message.From.FirstName} was deleted in chat {message.Chat.Title}.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting message");
        }
    }
}

// Метод обработки других обновлений
async Task OnUpdate(Update update)
{
    // Реализуйте обработку других типов обновлений, если это необходимо
}
