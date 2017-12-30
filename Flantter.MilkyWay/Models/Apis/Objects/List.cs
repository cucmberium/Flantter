namespace Flantter.MilkyWay.Models.Apis.Objects
{
    public class List
    {
        public List(CoreTweet.List cList)
        {
            Description = cList.Description;
            FullName = cList.FullName;
            Name = cList.Name;
            Slug = cList.Slug;
            SubscriberCount = cList.SubscriberCount;
            MemberCount = cList.MemberCount;
            Mode = cList.Mode;
            Id = cList.Id;
            User = new User(cList.User);
        }

        public List()
        {
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

        #region Mode変更通知プロパティ

        public string Mode { get; set; }

        #endregion

        #region Id変更通知プロパティ

        public long Id { get; set; }

        #endregion

        #region User変更通知プロパティ

        public User User { get; set; }

        #endregion
    }
}