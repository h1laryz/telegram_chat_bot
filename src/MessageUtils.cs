using System;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;

namespace TelegramBot.Utils
{
    public static class MessageUtils
    {
        public static bool IsMessageFromThisChannel(Message message) => message.IsAutomaticForward;

        public static bool IsMessageFromChannel(Message message) => Convert.ToString(message.ForwardOrigin?.GetType()) == "Telegram.Bot.Types.MessageOriginChannel";

        public static string CleanText(string text)
        {
            text = text.Replace("(", "").Replace(")", "");
            var emojiPattern = new Regex(@"\p{So}|\p{Cs}\p{Cs}(\p{Cf}\p{Cs}\p{Cs})*");
            return emojiPattern.Replace(text, "");
        }

        public static bool ShouldBan(string text)
        {
            text = CleanText(text);
            var russianWordPattern = new Regex(@"[а-яА-ЯёЁ]+");
            var suspiciousPattern = new Regex(@"[^а-яА-ЯёЁ0-9\s.,!?\""\-()&+]+");
            var words = text.Split(new[] { ' ', '\t', '\n', '\r', ',' }, StringSplitOptions.RemoveEmptyEntries);
            int suspiciousWordsCount = 0;

            foreach (var word in words)
            {
                if (russianWordPattern.IsMatch(word) && suspiciousPattern.IsMatch(word))
                {
                    suspiciousWordsCount++;
                }
            }
            return suspiciousWordsCount >= 5;
        }

        public static bool IsBadChannel(string telegramChannelName)
        {
            return ContainsFootball(telegramChannelName) || ContainsFootball(telegramChannelName) || ContainsBasketball(telegramChannelName)
            || ContainsVolleyball(telegramChannelName) || ContainsTenis(telegramChannelName) || ContainsBets(telegramChannelName);
        }

        private static bool ContainsFootball(string telegramChannelName)
        {
            return telegramChannelName.ToLower().Contains("футбол");
        }

        private static bool ContainsBasketball(string telegramChannelName)
        {
            return telegramChannelName.ToLower().Contains("баскетбол");
        }

        private static bool ContainsVolleyball(string telegramChannelName)
        {
            return telegramChannelName.ToLower().Contains("волейбол");
        }

        private static bool ContainsTenis(string telegramChannelName)
        {
            return telegramChannelName.ToLower().Contains("тенис") || telegramChannelName.ToLower().Contains("теннис");
        }

        private static bool ContainsBets(string telegramChannelName)
        {
            return telegramChannelName.ToLower().Contains("ставки") || telegramChannelName.ToLower().Contains("прогнозы")
            || telegramChannelName.ToLower().Contains("322");
        }
    }
}
