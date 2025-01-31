using System;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using Telegram.Bot.Types;

namespace TelegramBot.Utils
{
    public static class MessageUtils
    {
        private readonly static Regex _suspiciousRegex = new Regex(@"[^а-яА-ЯёЁ0-9\s.,!?\""\-()&+]+");
        private readonly static Regex _cyrillicRegex = new Regex(@"\p{IsCyrillic}+");
        private readonly static Regex _emojiPattern = new Regex(@"\p{So}|\p{Cs}\p{Cs}(\p{Cf}\p{Cs}\p{Cs})*");
        private readonly static Regex _punctuationPattern = new Regex(@"[^\w\s]");
        private readonly static Regex _latinPattern = new Regex("^[\u0000-\u007F]+$");

        public static bool IsMessageFromThisChannel(Message message) => message.IsAutomaticForward;

        public static bool IsMessageFromChannel(Message message) => Convert.ToString(message.ForwardOrigin?.GetType()) == "Telegram.Bot.Types.MessageOriginChannel";

        public static string CleanText(string text)
        {
            text = _emojiPattern.Replace(text, "");
            text = _punctuationPattern.Replace(text, " ");

            return text.Trim();
        }

        public static bool ContainsAdmin(string text)
        {
            return text.Contains("админ", StringComparison.OrdinalIgnoreCase) 
                || text.Contains("admin", StringComparison.OrdinalIgnoreCase) 
                || text.Contains("адмін", StringComparison.OrdinalIgnoreCase)
                || text.Contains("owner", StringComparison.OrdinalIgnoreCase)
                || text.Contains("власник", StringComparison.OrdinalIgnoreCase)
                || text.Contains("создатель", StringComparison.OrdinalIgnoreCase);
        }

        public static bool ShouldBan(string text)
        {
            text = CleanText(text);
            
            var words = text.Split(new[] { ' ', '\t', '\n', '\r', ',' }, StringSplitOptions.RemoveEmptyEntries);
            int suspiciousWordsCount = 0;

            foreach (var word in words)
            {
                if (IsWordCyryllic(word) && IsWordSuspicious(word))
                {
                    ++suspiciousWordsCount;
                }
            }
            return suspiciousWordsCount >= 5;
        }

        public static bool IsWordCyryllic(string word)
        {
            return _cyrillicRegex.IsMatch(CleanText(word));
        }

        public static bool IsWordLatin(string word)
        {
            return _latinPattern.IsMatch(CleanText(word));
        }

        public static bool IsWordSuspicious(string word)
        {
            return _suspiciousRegex.IsMatch(CleanText(word));
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
