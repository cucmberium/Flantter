using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Twitter.Objects
{
    public class List
    {
        public List(CoreTweet.List cList)
        {
            this.Description = cList.Description;
            this.FullName = cList.FullName;
            this.Name = cList.Name;
            this.Slug = cList.Slug;
            this.SubscriberCount = cList.SubscriberCount;
            this.MemberCount = cList.MemberCount;
            this.Id = cList.Id;
            this.User = new User(cList.User);
        }

        #region Description変更通知プロパティ
        public string Description { get; set; }
        #endregion

        #region Name変更通知プロパティ
        public string Name { get; set; }
        #endregion

        #region FullName変更通知プロパティ
        public string FullName { get; set; }
        #endregion

        #region Slug変更通知プロパティ
        public string Slug { get; set; }
        #endregion

        #region SubscriberCount変更通知プロパティ
        public int SubscriberCount { get; set; }
        #endregion

        #region MemberCount変更通知プロパティ
        public int MemberCount { get; set; }
        #endregion

        #region Id変更通知プロパティ
        public long Id { get; set; }
        #endregion

        #region User変更通知プロパティ
        public User User { get; set; }
        #endregion
    }
}
