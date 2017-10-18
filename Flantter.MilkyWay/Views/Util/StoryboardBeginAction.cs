using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using Microsoft.Xaml.Interactivity;

namespace Flantter.MilkyWay.Views.Util
{
    public class StoryboardBeginAction : DependencyObject, IAction
    {
        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("Key", typeof(string), typeof(StoryboardBeginAction),
                new PropertyMetadata(null));

        public string Key
        {
            get => GetValue(KeyProperty) as string;
            set => SetValue(KeyProperty, value);
        }

        public object Execute(object sender, object parameter)
        {
            var obj = sender as FrameworkElement;
            if (obj == null)
                return null;

            if (obj.Resources.ContainsKey(Key))
                ((Storyboard) obj.Resources[Key]).Begin();

            return null;
        }
    }
}