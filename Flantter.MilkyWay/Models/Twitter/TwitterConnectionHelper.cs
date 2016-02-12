using CoreTweet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Twitter
{
    public static class TwitterConnectionHelper
    {
        private const string Twitter_for_iPhone = "IQKbtAYlXLripLGPWd0HUA";
        private const string Twitter_for_Android = "3nVuSoBZnx6U4vzUxf5w";
        private const string Twitter_for_Google_TV = "iAtYJ4HpUVfIUoNnif1DA";
        private const string Twitter_for_iPad = "CjulERsDeqhhjSme66ECg";
        private const string Twitter_for_Mac = "3rJOl1ODzm9yZy63FACdg";
        private const string Twitter_for_Windows_Phone = "yN3DUNVO0Me63IAQdhTfCA";
        private const string Twitter_for_Windows = "TgHNMa7WZE7Cxi1JbkAMQ";
        private const string TweetDeck = "yT577ApRtZw51q4NPMPPOQ";

        public static readonly List<string> OfficialConsumerKeyList = new List<string>() { Twitter_for_iPhone, Twitter_for_Android, Twitter_for_Google_TV, Twitter_for_iPad, Twitter_for_Mac, Twitter_for_Windows_Phone, TweetDeck };

        public static string GetUserAgent(Tokens tokens)
        {
            switch (tokens.ConsumerKey)
            {
                case Twitter_for_Android:
                    // Nexus One に偽装
                    return "TwitterAndroid /5.2.4 (524-r1) Nexus One/8 (HTC;passion;google;passion;0)";
                case Twitter_for_iPhone:
                    return "Twitter-iPhone";
                case Twitter_for_Google_TV:
                    return "Twitter-GoogleTV";
                case Twitter_for_iPad:
                    return "Twitter-iPad";
                case Twitter_for_Mac:
                    return "Twitter-Mac";
                case Twitter_for_Windows_Phone:
                    return "Twitter-WindowsPhone";
                case Twitter_for_Windows:
                    return "Twitter-Windows";
                case TweetDeck:
                    // Microsoft Edge に偽装
                    return "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Safari/537.36 Edge/13.14257";
                default:
                    return "Flantter";
            }
        }
    }
}


