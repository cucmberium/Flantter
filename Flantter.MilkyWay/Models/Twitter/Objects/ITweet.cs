using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Twitter.Objects
{
    public interface ITweet
    {
        long Id { get; set; }
        DateTime CreatedAt { get; set; }
    }
}
