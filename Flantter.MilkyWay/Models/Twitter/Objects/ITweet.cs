using System;

namespace Flantter.MilkyWay.Models.Twitter.Objects
{
    public interface ITweet
    {
        long Id { get; set; }
        DateTime CreatedAt { get; set; }
    }
}