using System;
using System.Collections.Generic;
using System.Linq;

namespace Flantter.MilkyWay.Models.Apis.Objects
{
    public class DirectMessage : ITweet
    {
        public DirectMessage(CoreTweet.DirectMessage cDirectMessage)
        {
            CreatedAt = cDirectMessage.CreatedAt.DateTime;
            Entities = new Entities(cDirectMessage.Entities, null);
            Id = cDirectMessage.Id;
            Text = cDirectMessage.Text;
            Recipient = new User(cDirectMessage.Recipient);
            Sender = new User(cDirectMessage.Sender);
            PossiblySensitive = false;
        }

        public DirectMessage(TootNet.Objects.Status cDirectMessage, TootNet.Objects.Account cRecipient)
        {
            CreatedAt = cDirectMessage.CreatedAt;

            var urlEntities = new List<string>();
            var text = cDirectMessage.Content.Replace("<br />", "\n");
            text = Status.LinkRegex.Replace(text, match =>
            {
                var userMention = cDirectMessage.Mentions.Where(x =>
                        x.Url == match.Groups[2].Value || x.Url.Replace("/@", "/users/") == match.Groups[2].Value)
                    .ToArray();
                if (userMention.Length != 0)
                    return " @" + userMention.First().Acct + " ";

                urlEntities.Add(match.Groups[2].Value);
                return " " + match.Groups[1]?.Value + match.Groups[3].Value + " ";
            });
            text = Status.ContentRegex.Replace(text, "").Trim();
            text = EmojiPatterns.LightValidEmoji.Replace(text,
                x => EmojiPatterns.EmojiDictionary.TryGetValue(x.Groups[2].Value, out string val) ? val : x.Value);
            Text = text;

            Entities = new Entities(cDirectMessage.MediaAttachments, cDirectMessage.Mentions, cDirectMessage.Tags,
                urlEntities, cDirectMessage.Content);

            Id = cDirectMessage.Id;
            Recipient = new User(cRecipient);
            Sender = new User(cDirectMessage.Account);
            PossiblySensitive = cDirectMessage.Sensitive ?? false;
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