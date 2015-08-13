using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Twitter.Objects
{
    public class EventMessage : BindableBase, ITweet
    {
        public EventMessage(CoreTweet.Streaming.EventMessage cEventMessage)
        {
            this.CreatedAt = cEventMessage.CreatedAt.DateTime;
            this.Id = this.CreatedAt.ToBinary();
            this.Source = new User(cEventMessage.Source);
            this.Target = new User(cEventMessage.Target);
            this.TargetStatus = (cEventMessage.TargetStatus != null) ? new Status(cEventMessage.TargetStatus) : null;
            this.Type = cEventMessage.Type.ToString();
        }

        #region CreatedAt変更通知プロパティ
        private DateTime _CreatedAt;
        public DateTime CreatedAt { get { return this._CreatedAt; } set { this.SetProperty(ref this._CreatedAt, value); } }
        #endregion

        #region Source変更通知プロパティ
        private User _Source;
        public User Source { get { return this._Source; } set { this.SetProperty(ref this._Source, value); } }
        #endregion

        #region Target変更通知プロパティ
        private User _Target;
        public User Target { get { return this._Target; } set { this.SetProperty(ref this._Target, value); } }
        #endregion

        #region TargetStatus変更通知プロパティ
        private Status _TargetStatus;
        public Status TargetStatus { get { return this._TargetStatus; } set { this.SetProperty(ref this._TargetStatus, value); } }
        #endregion

        #region Type変更通知プロパティ
        private string _Type;
        public string Type { get { return this._Type; } set { this.SetProperty(ref this._Type, value); } }
        #endregion

        #region Id変更通知プロパティ
        private long _Id;
        public long Id { get { return this._Id; } set { this.SetProperty(ref this._Id, value); } }
        #endregion
    }
}
