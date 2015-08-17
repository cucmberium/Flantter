using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.ViewModels.Service
{
    public class Notice
    {
        private static Notice _Instance = new Notice();
        private Notice()
        {
            this.UserProfileShowCommand = new ReactiveCommand();
            this.MediaShowCommand = new ReactiveCommand();
            this.StatusDetailShowCommand = new ReactiveCommand();
            this.MentionLoadCommand = new ReactiveCommand();
            this.ReplyCommand = new ReactiveCommand();
            this.RetweetCommand = new ReactiveCommand();
            this.FavoriteCommand = new ReactiveCommand();
            this.UrlClickCommand = new ReactiveCommand();
            this.TweetDetailShowCommand = new ReactiveCommand();
            this.ReplyToAllCommand = new ReactiveCommand();
            this.DirectMessageSendCommand = new ReactiveCommand();
        }

        public static Notice Instance
        {
            get { return _Instance; }
        }

        public ReactiveCommand UserProfileShowCommand { get; private set; }
        public ReactiveCommand MediaShowCommand { get; private set; }
        public ReactiveCommand StatusDetailShowCommand { get; private set; }
        public ReactiveCommand MentionLoadCommand { get; private set; }
        public ReactiveCommand ReplyCommand { get; private set; }
        public ReactiveCommand RetweetCommand { get; private set; }
        public ReactiveCommand FavoriteCommand { get; private set; }
        public ReactiveCommand UrlClickCommand { get; private set; }
        public ReactiveCommand TweetDetailShowCommand { get; private set; }
        public ReactiveCommand ReplyToAllCommand { get; private set; }
        public ReactiveCommand DirectMessageSendCommand { get; private set; }
    }
}
