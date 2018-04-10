using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Flantter.MilkyWay.Views.Controls;
using Flantter.MilkyWay.Views.Util;
using Microsoft.Xaml.Interactivity;

namespace Flantter.MilkyWay.Views.Behaviors
{
    [ContentProperty(Name = "Triggers")]
    public class TextBoxKeyTriggerBehavior : DependencyObject, IBehavior
    {
        public static readonly DependencyProperty TriggersProperty =
            DependencyProperty.Register("Triggers", typeof(ActionCollection), typeof(TextBoxKeyTriggerBehavior),
                new PropertyMetadata(null));

        private KeysEventArgs _keysEventArgs;

        private KeyEventHandler _keyupEventHandler;

        public ActionCollection Triggers
        {
            get
            {
                var triggers = (ActionCollection) GetValue(TriggersProperty);
                if (triggers == null)
                {
                    triggers = new ActionCollection();
                    SetValue(TriggersProperty, triggers);
                }
                return triggers;
            }
        }

        public DependencyObject AssociatedObject { get; set; }

        public void Attach(DependencyObject AssociatedObject)
        {
            this.AssociatedObject = AssociatedObject;

            _keysEventArgs = new KeysEventArgs();

            var uiElement = this.AssociatedObject as ExtendedTextBox;
            uiElement.PreKeyDown += UIElement_KeyDown;
            _keyupEventHandler = UIElement_KeyUp;
            uiElement.AddHandler(UIElement.KeyUpEvent, _keyupEventHandler, true);
            uiElement.IsEnabledChanged += UIElement_IsEnabledChanged;
        }

        public void Detach()
        {
            var uiElement = AssociatedObject as ExtendedTextBox;
            uiElement.PreKeyDown -= UIElement_KeyDown;
            uiElement.RemoveHandler(UIElement.KeyUpEvent, _keyupEventHandler);
            uiElement.IsEnabledChanged -= UIElement_IsEnabledChanged;
        }

        private void UIElement_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (KeysEventArgs.Modifiers.Any(x => x == e.Key))
            {
                if (_keysEventArgs.ModifierCollection.All(x => x != e.Key))
                    _keysEventArgs.ModifierCollection.Add(e.Key);
            }
            else if (_keysEventArgs.KeyCollection.All(x => x != e.Key))
            {
                _keysEventArgs.KeyCollection.Add(e.Key);
            }

            e.Handled = Interaction.ExecuteActions(AssociatedObject, Triggers, _keysEventArgs).Any(x => (bool) x);

            if (e.Handled)
                _keysEventArgs.KeyCollection.Clear();
        }

        private void UIElement_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (_keysEventArgs.ModifierCollection.Any(x => x == e.Key))
                _keysEventArgs.ModifierCollection.Remove(e.Key);
            if (_keysEventArgs.KeyCollection.Any(x => x == e.Key))
                _keysEventArgs.KeyCollection.Remove(e.Key);
        }

        private void UIElement_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _keysEventArgs.ModifierCollection.Clear();
            _keysEventArgs.KeyCollection.Clear();
        }
    }
}