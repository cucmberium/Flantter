using Flantter.MilkyWay.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.ViewModels
{
	public enum TweetTypeEnum
	{
		None,
		Mention,
		Favorite,
		Retweet,
		MyStatus
	}

	public enum MediaTypeEnum
	{
		Picture,
		Video,
	}

	public enum EventTypeEnum
	{
		Favorite,
		Unfavorite,
		Follow,
		UserUpdate,
		Other
	}

	public class TweetViewModel
    {
        public TweetViewModel(TweetModel tweet)
        {
        }
    }
}
