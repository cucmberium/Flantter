using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Twitter.Objects
{
    public class DirectMessage : BindableBase, ITweet
    {
        public DirectMessage(CoreTweet.DirectMessage cDirectMessage)
        {
            this.CreatedAt = cDirectMessage.CreatedAt.DateTime;
            this.Entities = new Entities(cDirectMessage.Entities);
            this.Id = cDirectMessage.Id;
            this.Recipient = new User(cDirectMessage.Recipient);
            this.Sender = new User(cDirectMessage.Sender);
        }

        #region CreatedAt変更通知プロパティ
        private DateTime _CreatedAt;
        public DateTime CreatedAt { get { return this._CreatedAt; } set { this.SetProperty(ref this._CreatedAt, value); } }
        #endregion

        #region Entities変更通知プロパティ
        private Entities _Entities;
        public Entities Entities { get { return this._Entities; } set { this.SetProperty(ref this._Entities, value); } }
        #endregion

        #region Id変更通知プロパティ
        private long _Id;
        public long Id { get { return this._Id; } set { this.SetProperty(ref this._Id, value); } }
        #endregion

        #region Text変更通知プロパティ
        private string _Text;
        public string Text { get { return this._Text; } set { this.SetProperty(ref this._Text, value); } }
        #endregion

        #region Recipient変更通知プロパティ
        private User _Recipient;
        public User Recipient { get { return this._Recipient; } set { this.SetProperty(ref this._Recipient, value); } }
        #endregion

        #region Sender変更通知プロパティ
        private User _Sender;
        public User Sender { get { return this._Sender; } set { this.SetProperty(ref this._Sender, value); } }
        #endregion
    }
}
