using System;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using Serilog;
using Telegram.Bot.Types;

namespace TelegramBot.Utils
{
    public static class MessageUtils
    {
        private readonly static Regex _cyrillicRegex = new Regex(@"\p{IsCyrillic}+");
        private readonly static Regex _nonCyrillicRegex = new Regex(@"[^\p{IsCyrillic}]+");

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
                || text.Contains("создатель", StringComparison.OrdinalIgnoreCase)
                || text.Contains("владелец", StringComparison.OrdinalIgnoreCase);
        }

        public static bool ShouldBan(string text)
        {
            text = CleanText(text);
            
            var words = text.Split(new[] { ' ', '\t', '\n', '\r', ',' }, StringSplitOptions.RemoveEmptyEntries);
            int suspiciousWordsCount = 0;

            foreach (var word in words)
            {
                if (IsWordSuspicious(word))
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

        public static bool WordContainsCyryllic(string word)
        {
            word = CleanText(word);

            foreach (var c in word)
            {
                if (_cyrillicRegex.IsMatch(c.ToString()))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool WordContainsNonCyryllic(string word)
        {
            word = CleanText(word);

            foreach (var c in word)
            {
                if (_nonCyrillicRegex.IsMatch(c.ToString()))
                {
                    return true;
                }
            }

            return false;
        }


        public static bool IsWordLatin(string word)
        {
            return _latinPattern.IsMatch(CleanText(word));
        }

        public static bool IsWordSuspicious(string word)
        {
            return WordContainsCyryllic(word) && WordContainsNonCyryllic(word);
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
