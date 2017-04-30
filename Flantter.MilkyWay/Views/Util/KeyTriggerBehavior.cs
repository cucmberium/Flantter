using System;
using System.Collections.Generic;
using System.Linq;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Microsoft.Xaml.Interactivity;

namespace Flantter.MilkyWay.Views.Util
{
    public class KeysEventArgs : EventArgs
    {
        public KeysEventArgs()
        {
            KeyCollection = new List<VirtualKey>();
        }

        public List<VirtualKey> KeyCollection { get; }
    }

    [ContentProperty(Name = "Triggers")]
    public class KeyTriggerBehavior : DependencyObject, IBehavior
    {
        public static readonly DependencyProperty TriggersProperty =
            DependencyProperty.Register("Triggers", typeof(ActionCollection), typeof(KeyTriggerBehavior),
                new PropertyMetadata(null));

        private KeyEventHandler _keydownEventHandler;

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

        public void Attach(DependencyObject associatedObject)
        {
            AssociatedObject = associatedObject;

            _keysEventArgs = new KeysEventArgs();

            var uiElement = AssociatedObject as UIElement;
            _keyupEventHandler = UIElement_KeyUp;
            _keydownEventHandler = UIElement_KeyDown;
            uiElement.AddHandler(UIElement.KeyDownEvent, _keydownEventHandler, true);
            uiElement.AddHandler(UIElement.KeyUpEvent, _keyupEventHandler, true);

            var control = AssociatedObject as Control;
            if (control != null)
                control.IsEnabledChanged += Control_IsEnabledChanged;
        }

        public void Detach()
        {
            var uiElement = AssociatedObject as UIElement;
            uiElement.RemoveHandler(UIElement.KeyDownEvent, _keydownEventHandler);
            uiElement.RemoveHandler(UIElement.KeyUpEvent, _keyupEventHandler);

            var control = AssociatedObject as Control;
            if (control != null)
                control.IsEnabledChanged -= Control_IsEnabledChanged;
        }

        private void UIElement_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Handled)
                return;

            if (e.Key == VirtualKey.Enter)
            {
                if (e.KeyStatus.RepeatCount != 1)
                    return;

                if (_keysEventArgs.KeyCollection.All(x => x != e.Key))
                    _keysEventArgs.KeyCollection.Add(e.Key);

                e.Handled = Interaction.ExecuteActions(AssociatedObject, Triggers, _keysEventArgs).Any(x => (bool) x);
            }
            else
            {
                if (_keysEventArgs.KeyCollection.All(x => x != e.Key))
                    _keysEventArgs.KeyCollection.Add(e.Key);

                e.Handled = Interaction.ExecuteActions(AssociatedObject, Triggers, _keysEventArgs).Any(x => (bool) x);
            }

            if (e.Handled)
                _keysEventArgs.KeyCollection.Clear();
        }

        private void UIElement_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (_keysEventArgs.KeyCollection.Any(x => x == e.Key))
                _keysEventArgs.KeyCollection.Remove(e.Key);
        }

        private void Control_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _keysEventArgs.KeyCollection.Clear();
        }
    }

    [ContentProperty(Name = "Triggers")]
    public class GlobalKeyTriggerBehavior : DependencyObject, IBehavior
    {
        public static readonly DependencyProperty TriggersProperty =
            DependencyProperty.Register("Triggers", typeof(ActionCollection), typeof(KeyTriggerBehavior),
                new PropertyMetadata(null));

        private KeyEventHandler _keydownHandler;

        private KeysEventArgs _keysEventArgs;
        private KeyEventHandler _keyUpHandler;

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

        public void Attach(DependencyObject associatedObject)
        {
            AssociatedObject = associatedObject;

            _keysEventArgs = new KeysEventArgs();

            _keydownHandler = UIElement_KeyDown;
            _keyUpHandler = UIElement_KeyUp;
            Window.Current.Content.AddHandler(UIElement.KeyDownEvent, _keydownHandler, true);
            Window.Current.Content.AddHandler(UIElement.KeyUpEvent, _keyUpHandler, true);
        }

        public void Detach()
        {
            Window.Current.Content.RemoveHandler(UIElement.KeyDownEvent, _keydownHandler);
            Window.Current.Content.RemoveHandler(UIElement.KeyUpEvent, _keyUpHandler);
        }

        private void UIElement_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Handled)
                return;

            if (e.OriginalSource is TextBox || e.OriginalSource is SearchBox || e.OriginalSource is PasswordBox ||
                e.OriginalSource is RichEditBox)
                return;

            if (e.Key == VirtualKey.Enter)
            {
                if (e.KeyStatus.RepeatCount != 1)
                    return;

                if (_keysEventArgs.KeyCollection.All(x => x != e.Key))
                    _keysEventArgs.KeyCollection.Add(e.Key);

                e.Handled = Interaction.ExecuteActions(AssociatedObject, Triggers, _keysEventArgs).Any(x => (bool) x);
            }
            else
            {
                if (_keysEventArgs.KeyCollection.All(x => x != e.Key))
                    _keysEventArgs.KeyCollection.Add(e.Key);

                e.Handled = Interaction.ExecuteActions(AssociatedObject, Triggers, _keysEventArgs).Any(x => (bool) x);
            }

            if (e.Handled)
                _keysEventArgs.KeyCollection.Clear();
        }

        private void UIElement_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (_keysEventArgs.KeyCollection.Any(x => x == e.Key))
                _keysEventArgs.KeyCollection.Remove(e.Key);
        }
    }

    [ContentProperty(Name = "Actions")]
    public sealed class KeyTrigger : DependencyObject, IAction
    {
        public static readonly DependencyProperty ActionsProperty =
            DependencyProperty.Register("Actions", typeof(ActionCollection), typeof(KeyTrigger),
                new PropertyMetadata(null));

        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("Key", typeof(string), typeof(KeyTrigger),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ModifiersProperty =
            DependencyProperty.Register("Modifiers", typeof(string), typeof(KeyTrigger),
                new PropertyMetadata(null));

        public static readonly DependencyProperty HandledProperty =
            DependencyProperty.Register("Handled", typeof(bool), typeof(KeyTrigger),
                new PropertyMetadata(true));

        public ActionCollection Actions
        {
            get
            {
                var actions = (ActionCollection) GetValue(ActionsProperty);
                if (actions == null)
                {
                    actions = new ActionCollection();
                    SetValue(ActionsProperty, actions);
                }
                return actions;
            }
        }

        public string Key
        {
            get => GetValue(KeyProperty) as string;
            set => SetValue(KeyProperty, value);
        }

        public string Modifiers
        {
            get => GetValue(ModifiersProperty) as string;
            set => SetValue(ModifiersProperty, value);
        }

        public bool Handled
        {
            get => (bool) GetValue(HandledProperty);
            set => SetValue(HandledProperty, value);
        }

        public object Execute(object sender, object parameter)
        {
            var keysEventArgs = parameter as KeysEventArgs;
            if (keysEventArgs == null)
                return false;

            if (keysEventArgs.KeyCollection.All(x => x.ToString("F") != Key))
                return false;

            if (!string.IsNullOrWhiteSpace(Modifiers) &&
                keysEventArgs.KeyCollection.All(x => x.ToString("F") != Modifiers))
                return false;

            Interaction.ExecuteActions(sender, Actions, null);

            return Handled;
        }
    }
}