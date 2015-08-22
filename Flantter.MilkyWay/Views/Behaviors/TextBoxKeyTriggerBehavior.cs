using Flantter.MilkyWay.Views.Controls;
using Flantter.MilkyWay.Views.Util;
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

namespace Flantter.MilkyWay.Views.Behaviors
{
    [ContentProperty(Name = "Triggers")]
    public class TextBoxKeyTriggerBehavior : DependencyObject, IBehavior
    {
        public static readonly DependencyProperty TriggersProperty =
            DependencyProperty.Register("Triggers", typeof(ActionCollection), typeof(TextBoxKeyTriggerBehavior), new PropertyMetadata(null));
        
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

            var uiElement = this.AssociatedObject as ExtendedTextBox;
            uiElement.PreKeyDown += UIElement_KeyDown;
            uiElement.AddHandler(UIElement.KeyUpEvent, new KeyEventHandler(UIElement_KeyUp), true);
        }
        
        public void Detach()
        {
        }
        
        private void UIElement_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (!keysEventArgs.KeyCollection.Any(x => x == e.Key))
                keysEventArgs.KeyCollection.Add(e.Key);

            e.Handled = Interaction.ExecuteActions(AssociatedObject, this.Triggers, keysEventArgs).Any(x => (bool)x == true);
        }

        private void UIElement_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (keysEventArgs.KeyCollection.Any(x => x == e.Key))
                keysEventArgs.KeyCollection.Remove(e.Key);
        }
    }
}
