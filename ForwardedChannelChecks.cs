using System;

namespace Checks
{
    class ForwardedChannelChecks
    {
    public static bool FullCheck(String telegramChannelName)
    {
        return ContainsFootball(telegramChannelName) || ContainsFootball(telegramChannelName) || ContainsBasketball(telegramChannelName)
        || ContainsVolleyball(telegramChannelName) || ContainsTenis(telegramChannelName) || ContainsBets(telegramChannelName);
    }

    public static bool ContainsFootball(String telegramChannelName)
    {
        return telegramChannelName.ToLower().Contains("футбол");
    }

    public static bool ContainsBasketball(String telegramChannelName)
    {
        return telegramChannelName.ToLower().Contains("баскетбол");
    }

    public static bool ContainsVolleyball(String telegramChannelName)
    {
        return telegramChannelName.ToLower().Contains("волейбол");
    }

    public static bool ContainsTenis(String telegramChannelName)
    {
        return telegramChannelName.ToLower().Contains("тенис") || telegramChannelName.ToLower().Contains("теннис");
    }

    public static bool ContainsBets(String telegramChannelName)
    {
        return telegramChannelName.ToLower().Contains("ставки") || telegramChannelName.ToLower().Contains("прогнозы")
        || telegramChannelName.ToLower().Contains("322");
    }
    };


    
}