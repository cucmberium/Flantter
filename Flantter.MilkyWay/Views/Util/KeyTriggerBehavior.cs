using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;

namespace Flantter.MilkyWay.Views.Util
{
    public class KeysEventArgs : EventArgs
    {
        public KeysEventArgs()
        {
            this.KeyCollection = new List<VirtualKey>();
        }

        public List<VirtualKey> KeyCollection { get; private set; }
    }

    [ContentProperty(Name = "Triggers")]
    public class KeyTriggerBehavior : DependencyObject, IBehavior
    {
        public static readonly DependencyProperty TriggersProperty =
            DependencyProperty.Register("Triggers", typeof(ActionCollection), typeof(KeyTriggerBehavior), new PropertyMetadata(null));
        
        public ActionCollection Triggers
        {
            get
            {
                ActionCollection triggers = (ActionCollection)base.GetValue(TriggersProperty);
                if (triggers == null)
                {
                    triggers = new ActionCollection();
                    base.SetValue(TriggersProperty, triggers);
                }
                return triggers;
            }
        }

        private DependencyObject _AssociatedObject;
        public DependencyObject AssociatedObject
        {
            get { return this._AssociatedObject; }
            set { this._AssociatedObject = value; }
        }

        private KeysEventArgs keysEventArgs;

        public void Attach(DependencyObject AssociatedObject)
        {
            this.AssociatedObject = AssociatedObject;

            keysEventArgs = new KeysEventArgs();

            var uiElement = this.AssociatedObject as UIElement;
            uiElement.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(UIElement_KeyDown), true);
            uiElement.AddHandler(UIElement.KeyUpEvent, new KeyEventHandler(UIElement_KeyUp), true);
        }
        
        public void Detach()
        {
        }
        
        private void UIElement_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && e.KeyStatus.RepeatCount == 1)
            {
                if (!keysEventArgs.KeyCollection.Any(x => x == e.Key))
                    keysEventArgs.KeyCollection.Add(e.Key);

                e.Handled = Interaction.ExecuteActions(AssociatedObject, this.Triggers, keysEventArgs).Any(x => (bool)x == true);
            }
            else
            {
                if (!keysEventArgs.KeyCollection.Any(x => x == e.Key))
                    keysEventArgs.KeyCollection.Add(e.Key);

                e.Handled = Interaction.ExecuteActions(AssociatedObject, this.Triggers, keysEventArgs).Any(x => (bool)x == true);
            }

            e.Handled = true;
        }

        private void UIElement_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (keysEventArgs.KeyCollection.Any(x => x == e.Key))
                keysEventArgs.KeyCollection.Remove(e.Key);
        }
    }

    [ContentProperty(Name = "Actions")]
    public sealed class KeyTrigger : DependencyObject, IAction
    {
        public static readonly DependencyProperty ActionsProperty =
            DependencyProperty.Register("Actions", typeof(ActionCollection), typeof(KeyTrigger),
                new PropertyMetadata(null));

        public ActionCollection Actions
        {
            get
            {
                ActionCollection actions = (ActionCollection)base.GetValue(ActionsProperty);
                if (actions == null)
                {
                    actions = new ActionCollection();
                    base.SetValue(ActionsProperty, actions);
                }
                return actions;
            }
        }

        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("Key", typeof(string), typeof(KeyTrigger),
                new PropertyMetadata(null));

        public string Key
        {
            get
            {
                return base.GetValue(KeyProperty) as string;
            }
            set
            {
                base.SetValue(KeyProperty, value);
            }
        }

        public static readonly DependencyProperty ModifiersProperty =
            DependencyProperty.Register("Modifiers", typeof(string), typeof(KeyTrigger),
                new PropertyMetadata(null));

        public string Modifiers
        {
            get
            {
                return base.GetValue(ModifiersProperty) as string;
            }
            set
            {
                base.SetValue(ModifiersProperty, value);
            }
        }

        public object Execute(object sender, object parameter)
        {
            var keysEventArgs = parameter as KeysEventArgs;
            if (keysEventArgs == null)
                return false;

            if (!keysEventArgs.KeyCollection.Any(x => x.ToString("F") == Key))
                return false;

            if (!string.IsNullOrWhiteSpace(Modifiers) && !keysEventArgs.KeyCollection.Any(x => x.ToString("F") == Modifiers))
                return false;

            Interaction.ExecuteActions(sender, this.Actions, null);

            return true;
        }
    }

}
