using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Twitter.Objects
{
    public class Collection
    {
        public Collection(CoreTweet.Timeline cCollection)
        {
            this.Description = cCollection.Description;
            this.Name = cCollection.Name;
            this.Url = cCollection.Url;
            this.Id = cCollection.Id;
            this.User = new User(cCollection.User);
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
            this.Status = new Status(cCollectionEntry.Tweet);
            this.SortIndex = cCollectionEntry.SortIndex;
        }
        
        #region Status変更通知プロパティ
        public Status Status { get; set; }
        #endregion

        #region SortIndex変更通知プロパティ
        public long SortIndex { get; set; }
        #endregion
        
        public long Id { get { return this.Status.Id; } set { this.Status.Id = value; } }
        
        public DateTime CreatedAt { get { return this.Status.CreatedAt; } set { this.Status.CreatedAt = value; } }
    }
}
