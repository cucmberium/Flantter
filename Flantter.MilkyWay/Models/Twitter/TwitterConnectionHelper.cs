using System.Collections.Generic;
using CoreTweet;

namespace Flantter.MilkyWay.Models.Twitter
{
    public static class TwitterConnectionHelper
    {
        private const string TwitterForIPhone = "IQKbtAYlXLripLGPWd0HUA";
        private const string TwitterForAndroid = "3nVuSoBZnx6U4vzUxf5w";
        private const string TwitterForGoogleTv = "iAtYJ4HpUVfIUoNnif1DA";
        private const string TwitterForIPad = "CjulERsDeqhhjSme66ECg";
        private const string TwitterForMac = "3rJOl1ODzm9yZy63FACdg";
        private const string TwitterForWindowsPhone = "yN3DUNVO0Me63IAQdhTfCA";
        private const string TwitterForWindows = "TgHNMa7WZE7Cxi1JbkAMQ";
        private const string TweetDeck = "yT577ApRtZw51q4NPMPPOQ";

        public static readonly List<string> OfficialConsumerKeyList = new List<string>
        {
            TwitterForIPhone,
            TwitterForAndroid,
            TwitterForGoogleTv,
            TwitterForIPad,
            TwitterForMac,
            TwitterForWindowsPhone,
            TweetDeck
        };

        public static string GetUserAgent(Tokens tokens)
        {
            switch (tokens.ConsumerKey)
            {
                case TwitterForAndroid:
                    // Nexus One に偽装 (From twidere)
                    return "TwitterAndroid /5.2.4 (524-r1) Nexus One/8 (HTC;passion;google;passion;0)";
                case TwitterForIPhone:
                    return "Twitter-iPhone";
                case TwitterForGoogleTv:
                    return "Twitter-GoogleTV";
                case TwitterForIPad:
                    return "Twitter-iPad";
                case TwitterForMac:
                    return "Twitter-Mac";
                case TwitterForWindowsPhone:
                    return "Twitter-WindowsPhone";
                case TwitterForWindows:
                    // 調べたヘッダがこれだった
                    return "TwitterForWindows/4.3.3.0";
                case TweetDeck:
                    // Microsoft Edge に偽装
                    return
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Safari/537.36 Edge/13.14257";
                default:
                    return "Flantter";
            }
        }
    }
}