using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using System.Text.RegularExpressions;

using var cts = new CancellationTokenSource();

var token = "7765829125:AAFJaXu7okkG2GdQWsKSL7r5uzBr_oRlW-0";
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

// Проверка текста
// Проверка текста
bool ShouldBan(string text)
{
    Console.WriteLine($"Проверка текста: \"{text}\"");

    // Удаляем все скобки ( и )
    text = text.Replace("(", "").Replace(")", "");

    if (string.IsNullOrWhiteSpace(text))
        return false;

    // Регулярное выражение для русского слова
    var russianWordPattern = new Regex(@"[а-яА-ЯёЁ]+");

    // Регулярное выражение для подозрительных символов
    var suspiciousPattern = new Regex(@"[^а-яА-ЯёЁ0-9\s.,!?\""\-()]");

    // Разбиваем текст на слова
    var words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

    // Слова, которые нужно проверять
    var targetWords = new[] { "сливаю", "капперов", "бесплатно" };
    int targetWordCount = words.Count(word => targetWords.Contains(word.ToLower()));

    // Проверяем, если есть хотя бы 2 из целевых слов
    if (targetWordCount >= 2)
    {
        return true; // Сообщение подозрительное
    }

    int suspiciousWordsCount = 0;

    foreach (var word in words)
    {
        // Проверяем, если слово целиком русское
        if (russianWordPattern.IsMatch(word))
        {
            Console.WriteLine($"{word} is russian.");
            // Проверяем, есть ли подозрительные символы в русском слове
            if (suspiciousPattern.IsMatch(word))
            {
                Console.WriteLine($"{word} is suspicious.");
                ++suspiciousWordsCount;
            }
            else
            {
                Console.WriteLine($"{word} is not suspicious.");
            }
        }
    }

    return suspiciousWordsCount >= 2;
}


// Метод обработки ошибок
async Task OnError(Exception exception, HandleErrorSource source)
{
    Console.WriteLine(exception); // просто вывод ошибки в консоль
}

// Метод обработки сообщений
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

    if (message.Text != null && ShouldBan(message.Text))
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
                Console.WriteLine($"Skipped banning admin/owner: {message.From.Username ?? message.From.FirstName}");
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

            Console.WriteLine($"User {message.From.Username ?? message.From.FirstName} banned in {message.Chat.Title}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error banning user: {ex.Message}");
        }
    }
}

// Метод обработки других обновлений
async Task OnUpdate(Update update)
{
    // Реализуйте обработку других типов обновлений, если это необходимо
}
