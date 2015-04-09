using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Common
{
    public static class TwitterConsumerKeyPatterns
    {
        private const string Twitter_for_iPhone = "IQKbtAYlXLripLGPWd0HUA";
        private const string Twitter_for_Android = "3nVuSoBZnx6U4vzUxf5w";
        private const string Twitter_for_Google_TV = "iAtYJ4HpUVfIUoNnif1DA";
        private const string Twitter_for_iPad = "CjulERsDeqhhjSme66ECg";
        private const string Twitter_for_Mac = "3rJOl1ODzm9yZy63FACdg";
        private const string Twitter_for_Windows_Phone = "yN3DUNVO0Me63IAQdhTfCA";
        private const string TweetDeck = "yT577ApRtZw51q4NPMPPOQ";
        public static readonly List<string> OfficialConsumerKeyList = new List<string>() { Twitter_for_iPhone, Twitter_for_Android, Twitter_for_Google_TV, Twitter_for_iPad, Twitter_for_Mac, Twitter_for_Windows_Phone, TweetDeck };
    }
}
