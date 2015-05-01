using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels;
using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class TweetBackgroundColorBehavior
	{
        public static double GetTweetBrushAlpha(DependencyObject obj)
        {
            return (double)obj.GetValue(TweetBrushAlphaProperty);
        }
        public static void SetTweetBrushAlpha(DependencyObject obj, double value)
        {
            obj.SetValue(TweetBrushAlphaProperty, value);
        }

        public static SolidColorBrush GetFavoriteBackground(DependencyObject obj)
        {
            return (SolidColorBrush)obj.GetValue(FavoriteBackgroundProperty);
        }
        public static void SetFavoriteBackground(DependencyObject obj, SolidColorBrush value)
        {
            obj.SetValue(FavoriteBackgroundProperty, value);
        }

        public static SolidColorBrush GetRetweetBackground(DependencyObject obj)
        {
            return (SolidColorBrush)obj.GetValue(RetweetBackgroundProperty);
        }
        public static void SetRetweetBackground(DependencyObject obj, SolidColorBrush value)
        {
            obj.SetValue(RetweetBackgroundProperty, value);
        }

        public static SolidColorBrush GetMyStatusBackground(DependencyObject obj)
        {
            return (SolidColorBrush)obj.GetValue(MyStatusBackgroundProperty);
        }
        public static void SetMyStatusBackground(DependencyObject obj, SolidColorBrush value)
        {
            obj.SetValue(MyStatusBackgroundProperty, value);
        }

        public static SolidColorBrush GetMentionBackground(DependencyObject obj)
        {
            return (SolidColorBrush)obj.GetValue(MentionBackgroundProperty);
        }
        public static void SetMentionBackground(DependencyObject obj, SolidColorBrush value)
        {
            obj.SetValue(MentionBackgroundProperty, value);
        }
		
		public static SolidColorBrush GetDefaultBackground(DependencyObject obj)
		{
			return (SolidColorBrush)obj.GetValue(DefaultBackgroundProperty);
		}
		public static void SetDefaultBackground(DependencyObject obj, SolidColorBrush value)
		{
			obj.SetValue(DefaultBackgroundProperty, value);
		}

		public static TweetTypeEnum GetTweetType(DependencyObject obj)
        {
            return (TweetTypeEnum)obj.GetValue(TweetTypeProperty);
        }
        public static void SetTweetType(DependencyObject obj, TweetTypeEnum value)
        {
            obj.SetValue(TweetTypeProperty, value);
        }

		public static readonly DependencyProperty TweetBrushAlphaProperty =
            DependencyProperty.RegisterAttached("TweetBrushAlpha", typeof(double), typeof(TweetBackgroundColorBehavior), new PropertyMetadata(255.0, PropertyChanged));
        public static readonly DependencyProperty FavoriteBackgroundProperty =
            DependencyProperty.RegisterAttached("FavoriteBackground", typeof(SolidColorBrush), typeof(TweetBackgroundColorBehavior), new PropertyMetadata(null, PropertyChanged));
        public static readonly DependencyProperty RetweetBackgroundProperty =
            DependencyProperty.RegisterAttached("RetweetBackground", typeof(SolidColorBrush), typeof(TweetBackgroundColorBehavior), new PropertyMetadata(null, PropertyChanged));
        public static readonly DependencyProperty MyStatusBackgroundProperty =
            DependencyProperty.RegisterAttached("MyStatusBackground", typeof(SolidColorBrush), typeof(TweetBackgroundColorBehavior), new PropertyMetadata(null, PropertyChanged));
        public static readonly DependencyProperty MentionBackgroundProperty =
            DependencyProperty.RegisterAttached("MentionBackground", typeof(SolidColorBrush), typeof(TweetBackgroundColorBehavior), new PropertyMetadata(null, PropertyChanged));
		public static readonly DependencyProperty DefaultBackgroundProperty =
			DependencyProperty.RegisterAttached("DefaultBackground", typeof(SolidColorBrush), typeof(TweetBackgroundColorBehavior), new PropertyMetadata(null, PropertyChanged));
		public static readonly DependencyProperty TweetTypeProperty =
            DependencyProperty.RegisterAttached("TweetType", typeof(TweetTypeEnum), typeof(TweetBackgroundColorBehavior), new PropertyMetadata(TweetTypeEnum.None, PropertyChanged));

        public static void PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var grid = obj as Panel;
            if (grid == null)
                return;

            var alpha = Convert.ToByte(GetTweetBrushAlpha(obj));
            SolidColorBrush brush;

            var tweetType = GetTweetType(obj);

            switch (tweetType)
            {
                case TweetTypeEnum.Mention:
                    brush = GetMentionBackground(obj);
                    break;
                case TweetTypeEnum.MyStatus:
                    brush = GetMyStatusBackground(obj);
                    break;
                case TweetTypeEnum.Favorite:
                    brush = GetFavoriteBackground(obj);
                    break;
                case TweetTypeEnum.Retweet:
                    brush = GetRetweetBackground(obj);
                    break;
                default:
					brush = GetDefaultBackground(obj);
                    return;
            }

			if (brush == null)
			{
				brush = new SolidColorBrush();
				grid.Background = brush;
			}
			else
			{
				brush.Color = Color.FromArgb(alpha, brush.Color.R, brush.Color.G, brush.Color.B);
				grid.Background = brush;
			}
        }
    }
}
