namespace Flantter.MilkyWay.Models.Apis.Objects
{
    public class Relationship
    {
        public Relationship(CoreTweet.Relationship cRelationship)
        {
            Target = new RelationshipTarget(cRelationship.Target);
            Source = new RelationshipSource(cRelationship.Source);
        }

        public Relationship(TootNet.Objects.Relationship cRelationship)
        {
            Target = new RelationshipTarget();
            Source = new RelationshipSource(cRelationship);
        }

        public Relationship()
        {
        }

        public RelationshipTarget Target { get; set; }

        public RelationshipSource Source { get; set; }
    }

    public class RelationshipTarget
    {
        public RelationshipTarget(CoreTweet.RelationshipTarget cRelationshipTarget)
        {
            Id = cRelationshipTarget.Id;
            ScreenName = cRelationshipTarget.ScreenName;
            IsFollowing = cRelationshipTarget.IsFollowing;
            IsFollowedBy = cRelationshipTarget.IsFollowedBy;
            IsFollowingReceived = cRelationshipTarget.IsFollowingReceived ?? false;
            IsFollowingRequested = cRelationshipTarget.IsFollowingRequested ?? false;
        }

        public RelationshipTarget()
        {
        }

        public long Id { get; set; }
        public string ScreenName { get; set; }
        public bool IsFollowing { get; set; }
        public bool IsFollowedBy { get; set; }
        public bool IsFollowingReceived { get; set; }
        public bool IsFollowingRequested { get; set; }
    }

    public class RelationshipSource
    {
        public RelationshipSource(CoreTweet.RelationshipSource cRelationshipSource)
        {
            Id = cRelationshipSource.Id;
            ScreenName = cRelationshipSource.ScreenName;
            IsFollowing = cRelationshipSource.IsFollowing;
            IsFollowedBy = cRelationshipSource.IsFollowedBy;
            IsFollowingReceived = cRelationshipSource.IsFollowingReceived ?? false;
            IsFollowingRequested = cRelationshipSource.IsFollowingRequested ?? false;
            CanDM = cRelationshipSource.CanDM;
            AllReplies = cRelationshipSource.AllReplies ?? false;
            WantsRetweets = cRelationshipSource.WantsRetweets ?? false;
            IsBlocking = cRelationshipSource.IsBlocking ?? false;
            IsBlockedBy = cRelationshipSource.IsBlockedBy ?? false;
            IsMarkedSpam = cRelationshipSource.IsMarkedSpam ?? false;
            IsNotificationsEnabled = cRelationshipSource.IsNotificationsEnabled ?? false;
            IsMuting = cRelationshipSource.IsMuting ?? false;
        }

        public RelationshipSource(TootNet.Objects.Relationship cRelationshipSource)
        {
            IsFollowing = cRelationshipSource.Following;
            IsFollowedBy = cRelationshipSource.FollowedBy;
            IsFollowingRequested = cRelationshipSource.Requested;
            IsBlocking = cRelationshipSource.Blocking;
            IsMuting = cRelationshipSource.Muting;
        }

        public RelationshipSource()
        {
        }

        public long Id { get; set; }
        public string ScreenName { get; set; }
        public bool IsFollowing { get; set; }
        public bool IsFollowedBy { get; set; }
        public bool IsFollowingReceived { get; set; }
        public bool IsFollowingRequested { get; set; }
        public bool CanDM { get; set; }
        public bool AllReplies { get; set; }
        public bool WantsRetweets { get; set; }
        public bool IsBlocking { get; set; }
        public bool IsBlockedBy { get; set; }
        public bool IsMarkedSpam { get; set; }
        public bool IsNotificationsEnabled { get; set; }
        public bool IsMuting { get; set; }
    }
}