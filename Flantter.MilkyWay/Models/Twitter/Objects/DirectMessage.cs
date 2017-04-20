using System;
using System.Text.RegularExpressions;

namespace Flantter.MilkyWay.Models.Twitter.Objects
{
    public class DirectMessage : ITweet
    {
        private static readonly Regex ContentRegex =
            new Regex(@"<(""[^""]*""|'[^']*'|[^'"">])*>", RegexOptions.Compiled);

        public DirectMessage(CoreTweet.DirectMessage cDirectMessage)
        {
            CreatedAt = cDirectMessage.CreatedAt.DateTime;
            Entities = new Entities(cDirectMessage.Entities, null);
            Id = cDirectMessage.Id;
            Text = cDirectMessage.Text;
            Recipient = new User(cDirectMessage.Recipient);
            Sender = new User(cDirectMessage.Sender);
        }

        public DirectMessage(Mastonet.Entities.Status cDirectMessage, Mastonet.Entities.Account cRecipient)
        {
            CreatedAt = cDirectMessage.CreatedAt;
            Entities = new Entities(cDirectMessage.MediaAttachments, cDirectMessage.Mentions, cDirectMessage.Tags,
                cDirectMessage.Content);
            Id = cDirectMessage.Id;
            Text = ContentRegex.Replace(cDirectMessage.Content, "");
            Recipient = new User(cRecipient);
            Sender = new User(cDirectMessage.Account);
        }

        #region Entities変更通知プロパティ

        public Entities Entities { get; set; }

        #endregion

        #region Text変更通知プロパティ

        public string Text { get; set; }

        #endregion

        #region Recipient変更通知プロパティ

        public User Recipient { get; set; }

        #endregion

        #region Sender変更通知プロパティ

        public User Sender { get; set; }

        #endregion

        #region CreatedAt変更通知プロパティ

        public DateTime CreatedAt { get; set; }

        #endregion

        #region Id変更通知プロパティ

        public long Id { get; set; }

        #endregion
    }
}