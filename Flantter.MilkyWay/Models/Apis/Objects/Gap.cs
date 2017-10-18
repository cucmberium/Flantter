using System;

namespace Flantter.MilkyWay.Models.Apis.Objects
{
    public class Gap : ITweet
    {
        public Gap(long id, long maxId, DateTime createAt)
        {
            CreatedAt = createAt;
            Id = id;
            MaxId = maxId;
        }

        public Gap()
        {
        }

        public long MaxId { get; set; }

        public DateTime CreatedAt { get; set; }

        public long Id { get; set; }
    }
}