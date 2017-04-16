using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Twitter.Objects
{
    public class Relationship
    {
        public Relationship(CoreTweet.Relationship cRelationship)
        {
            this.Target = new RelationshipTarget(cRelationship.Target);
            this.Source = new RelationshipSource(cRelationship.Source);
        }

        public Relationship(Mastonet.Entities.Relationship cRelationship)
        {
            this.Target = new RelationshipTarget();
            this.Source = new RelationshipSource(cRelationship);
        }

        public RelationshipTarget Target { get; set; }
        
        public RelationshipSource Source { get; set; }
    }

    public class RelationshipTarget
    {
        public RelationshipTarget(CoreTweet.RelationshipTarget cRelationshipTarget)
        {
            this.Id = cRelationshipTarget.Id;
            this.ScreenName = cRelationshipTarget.ScreenName;
            this.IsFollowing = cRelationshipTarget.IsFollowing;
            this.IsFollowedBy = cRelationshipTarget.IsFollowedBy;
            this.IsFollowingReceived = cRelationshipTarget.IsFollowingReceived ?? false;
            this.IsFollowingRequested = cRelationshipTarget.IsFollowingRequested ?? false;
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
            this.Id = cRelationshipSource.Id;
            this.ScreenName = cRelationshipSource.ScreenName;
            this.IsFollowing = cRelationshipSource.IsFollowing;
            this.IsFollowedBy = cRelationshipSource.IsFollowedBy;
            this.IsFollowingReceived = cRelationshipSource.IsFollowingReceived ?? false;
            this.IsFollowingRequested = cRelationshipSource.IsFollowingRequested ?? false;
            this.CanDM = cRelationshipSource.CanDM;
            this.AllReplies = cRelationshipSource.AllReplies ?? false;
            this.WantsRetweets = cRelationshipSource.WantsRetweets ?? false;
            this.IsBlocking = cRelationshipSource.IsBlocking ?? false;
            this.IsBlockedBy = cRelationshipSource.IsBlockedBy ?? false;
            this.IsMarkedSpam = cRelationshipSource.IsMarkedSpam ?? false;
            this.IsNotificationsEnabled = cRelationshipSource.IsNotificationsEnabled ?? false;
            this.IsMuting = cRelationshipSource.IsMuting ?? false;
        }

        public RelationshipSource(Mastonet.Entities.Relationship cRelationshipSource)
        {
            this.IsFollowing = cRelationshipSource.Following;
            this.IsFollowedBy = cRelationshipSource.FollowedBy;
            this.IsFollowingRequested = cRelationshipSource.Requested;
            this.IsBlocking = cRelationshipSource.Blocking;
            this.IsMuting = cRelationshipSource.Muting;
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
