using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Serilog;
using TelegramBot.Utils;
using System.Text.RegularExpressions;
using System.Reflection.Metadata;

namespace TelegramBot.Services
{
    public class MessageHandler
    {
        private readonly TelegramBotClient _bot;
        private readonly CancellationToken _cancellationToken;

        public MessageHandler(TelegramBotClient bot, CancellationToken cancellationToken)
        {
            _bot = bot;
            _cancellationToken = cancellationToken;
        }

        public async Task HandleUpdateAsync(Update update)
        {
            if (update.Type != UpdateType.Message || update.Message == null)
                return;

            var message = update.Message;

            if (!string.IsNullOrEmpty(message.Text))
            {
                Log.Information($"Received message: {message.Text} ");

                if (message.Text[0] == '/')
                {
                    await HandleCommandsAsync(message);
                    return;
                }
            }
            else if (!string.IsNullOrEmpty(message.Caption))
            {
                Log.Information($"Received message: {message.Caption} ");
            }
            else
            {
                return;
            }
                
            if (message.Chat.Type != ChatType.Group && message.Chat.Type != ChatType.Supergroup)
                return;

            if (MessageUtils.IsMessageFromThisChannel(message))
            {
                Log.Information("Message is from a channel, skipping.");
                return;
            }

            if ((message.Text != null && MessageUtils.ShouldBan(message.Text)) || 
                (message.Caption != null && MessageUtils.ShouldBan(message.Caption)))
            {
                await BanUserAsync(message);
            }
            else if (MessageUtils.IsMessageFromChannel(message) && message.ForwardFromChat?.Title != null && MessageUtils.IsBadChannel(message.ForwardFromChat.Title))
            {
                await DeleteMessageAsync(message);
            }
        }

        private async Task HandleCommandsAsync(Message message)
        {
            if (message.Text == null || message.From == null || message.Chat == null)
            {
                return;
            }

            var chatId = message.Chat.Id;
            var userId = message.From.Id;

            if (message.Text.Contains("/help"))
            {
                string commandList = "Список доступных команд:\n\n" +
                             "/role <название> - назначить себе роль\n"; 
                             //+ "/old - узнать время, проведенное в чате"; // Добавьте другие команды по необходимости

                await _bot.SendMessage(chatId, commandList, replyParameters: message);
            }

            if (message.Text.Contains("/old"))
            {
                await _bot.SendMessage(chatId, $"скоро...", replyParameters: message);
            }

            if (message.Text.Contains("/role"))
            {
                if (!message.Text.Contains(' '))
                {
                    await _bot.SendMessage(chatId, "пример: /role dolbaeb", replyParameters: message);
                    return;
                }

                string roleName = message.Text.Substring(message.Text.IndexOf(' ') + 1).Trim();

                if (string.IsNullOrEmpty(roleName))
                {
                    await _bot.SendMessage(chatId, "Укажите название роли после команды. Пример: /role lox", replyParameters: message);
                    return;
                }

                if (!Regex.IsMatch(roleName, @"\p{IsCyrillic}+") && !Regex.IsMatch(roleName, "^[\u0000-\u007F]+$"))
                {
                    await _bot.SendMessage(chatId, $"Можно использовать только киррилицу/латиницу!", replyParameters: message);
                    return;
                }
                if (roleName.Contains("admin", StringComparison.OrdinalIgnoreCase) || roleName.Contains("админ", StringComparison.OrdinalIgnoreCase) || roleName.Contains("адмін", StringComparison.OrdinalIgnoreCase))
                {
                    await _bot.SendMessage(chatId, $"Ты кого наебать пытаешься, хуесос?", replyParameters: message);
                    return;
                }

                try
                {
                    await _bot.PromoteChatMember(chatId, userId, 
                    canChangeInfo: false, // Разрешение на изменение информации о чате
                    canPostMessages: false, // Разрешение на отправку сообщений
                    canEditMessages: false, // Разрешение на редактирование сообщений
                    canDeleteMessages: false, // Разрешение на удаление сообщений
                    canInviteUsers: true, // Разрешение на приглашение пользователей
                    canPinMessages: false, // Разрешение на закрепление сообщений
                    canPromoteMembers: false); // Разрешение на назначение других администраторов
                }
                catch (Exception exception)
                {
                    Log.Error($"Promotion error: ${exception.ToString()}");
                }

                await _bot.SetChatAdministratorCustomTitle(chatId, userId, roleName, _cancellationToken);

                await _bot.SendMessage(chatId, $"Теперь ваша роль - {roleName}!", ParseMode.None, message);
            }
        }

        private async Task DeleteMessageAsync(Message message)
        {
            try
            {
                if (message.From == null)
                {
                    return;
                }

                var chatMember = await _bot.GetChatMember(message.Chat.Id, message.From.Id, _cancellationToken);
                if (chatMember.Status == ChatMemberStatus.Administrator || chatMember.Status == ChatMemberStatus.Creator)
                {
                    Log.Information($"Skipped deleting admin/owner message: {message.From.Username ?? message.From.FirstName}");
                    return;
                }

                await _bot.DeleteMessage(message.Chat.Id, message.MessageId, _cancellationToken);
                Log.Information($"Message {message.From.Username ?? message.From.Id.ToString()} deleted in {message.Chat.Title}.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting msg");
            }
        }

        private async Task BanUserAsync(Message message)
        {
            try
            {
                if (message.From == null)
                {
                    return;
                }

                var chatMember = await _bot.GetChatMember(message.Chat.Id, message.From.Id, _cancellationToken);
                if (chatMember.Status == ChatMemberStatus.Administrator || chatMember.Status == ChatMemberStatus.Creator)
                {
                    Log.Information($"Skipped banning admin/owner: {message.From.Username ?? message.From.FirstName}");
                    return;
                }

                await _bot.BanChatMember(message.Chat.Id, message.From.Id, cancellationToken: _cancellationToken);
                await _bot.DeleteMessage(message.Chat.Id, message.MessageId, _cancellationToken);
                Log.Information($"User {message.From.Username ?? message.From.Id.ToString()} banned in {message.Chat.Title}.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error banning user");
            }
        }
    }
}