using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Serilog;
using TelegramBot.Utils;

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
            if (message.Text != null)
            {
                Log.Information($"Received message: {message.Text} ");
            }
            if (message.Caption != null)
            {
                Log.Information($"Received message: {message.Caption} ");
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

        private async Task DeleteMessageAsync(Message message)
        {
            try
            {
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