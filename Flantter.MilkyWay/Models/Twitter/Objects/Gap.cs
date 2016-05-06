using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Twitter.Objects
{
    public class Gap : ITweet
    {
        public Gap(long id, long maxId, DateTime createAt)
        {
            this.CreatedAt = createAt;
            this.Id = id;
            this.MaxId = maxId;
        }

        public DateTime CreatedAt { get; set; }

        public long Id { get; set; }

        public long MaxId { get; set; }
    }
}
