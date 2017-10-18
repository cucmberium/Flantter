using System;

namespace Flantter.MilkyWay.Models.Apis.Objects
{
    public interface ITweet
    {
        long Id { get; set; }
        DateTime CreatedAt { get; set; }
    }
}