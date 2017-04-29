using System;

namespace Flantter.MilkyWay.Models.Twitter.Objects
{
    public class Collection
    {
        public Collection(CoreTweet.Timeline cCollection)
        {
            Description = cCollection.Description;
            Name = cCollection.Name;
            Url = cCollection.Url;
            Id = cCollection.Id;
            User = new User(cCollection.User);
        }

        public Collection()
        {
        }

        #region Description変更通知プロパティ

        public string Description { get; set; }

        #endregion

        #region Name変更通知プロパティ

        public string Name { get; set; }

        #endregion

        #region Url変更通知プロパティ

        public string Url { get; set; }

        #endregion

        #region Id変更通知プロパティ

        public string Id { get; set; }

        #endregion

        #region User変更通知プロパティ

        public User User { get; set; }

        #endregion
    }

    public class CollectionEntry : ITweet
    {
        public CollectionEntry(CoreTweet.TimelineEntry cCollectionEntry)
        {
            Status = new Status(cCollectionEntry.Tweet);
            SortIndex = cCollectionEntry.SortIndex;
        }

        public CollectionEntry()
        {
        }

        #region Status変更通知プロパティ

        public Status Status { get; set; }

        #endregion

        #region SortIndex変更通知プロパティ

        public long SortIndex { get; set; }

        #endregion

        public long Id
        {
            get => Status.Id;
            set => Status.Id = value;
        }

        public DateTime CreatedAt
        {
            get => Status.CreatedAt;
            set => Status.CreatedAt = value;
        }
    }
}