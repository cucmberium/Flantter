using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flantter.MilkyWay.Views.Contents
{
    public sealed partial class FollowButon : UserControl
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(FollowButon),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(FollowButon),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ButtonContentProperty =
            DependencyProperty.Register("ButtonContent", typeof(object), typeof(FollowButon),
                new PropertyMetadata("Follow", ButtonContentChanged));

        public static readonly DependencyProperty ButtonPointerOverContentProperty =
            DependencyProperty.Register("ButtonPointerOverContent", typeof(object), typeof(FollowButon),
                new PropertyMetadata("Follow", ButtonContentChanged));

        private bool _pointerOver;

        public FollowButon()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                FollowButton.PointerEntered += (_, __) =>
                {
                    _pointerOver = true;
                    ContentChange();
                };
                FollowButton.PointerExited += (_, __) =>
                {
                    _pointerOver = false;
                    ContentChange();
                };
                ContentChange();
            };
        }

        public ICommand Command
        {
            get => (ICommand) GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object ButtonContent
        {
            get => GetValue(ButtonContentProperty);
            set => SetValue(ButtonContentProperty, value);
        }

        public object ButtonPointerOverContent
        {
            get => GetValue(ButtonPointerOverContentProperty);
            set => SetValue(ButtonPointerOverContentProperty, value);
        }

        private static void ButtonContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var followButton = d as FollowButon;
            followButton.ContentChange();
        }

        public void ContentChange()
        {
            FollowButton.Content = !_pointerOver ? ButtonContent : ButtonPointerOverContent;
        }

        private void FollowButton_Click(object sender, RoutedEventArgs e)
        {
            if (Command != null && Command.CanExecute(CommandParameter))
                Command.Execute(CommandParameter);
        }
    }
}