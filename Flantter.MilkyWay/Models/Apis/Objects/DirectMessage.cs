using System;

namespace Flantter.MilkyWay.Models.Apis.Objects
{
    public class DirectMessage : ITweet
    {
        public DirectMessage(CoreTweet.MessageCreateEvent cDirectMessage, CoreTweet.User cRecipient, CoreTweet.User cSender)
        {
            CreatedAt = cDirectMessage.CreatedTimestamp.DateTime;
            Entities = new Entities(cDirectMessage.MessageCreate.MessageData.Entities, null);
            Id = long.Parse(cDirectMessage.Id);
            Text = cDirectMessage.MessageCreate.MessageData.Text;
            Recipient = new User(cRecipient);
            Sender = new User(cSender);
            PossiblySensitive = false;
        }

        public DirectMessage()
        {
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

        #region PossiblySensitive変更通知プロパティ

        public bool PossiblySensitive { get; set; }

        #endregion

        #region Id変更通知プロパティ

        public long Id { get; set; }

        #endregion
    }
}