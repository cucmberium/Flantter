using Flantter.MilkyWay.Models.SettingsFlyouts;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Models.Twitter.Wrapper;
using Flantter.MilkyWay.ViewModels.Twitter.Objects;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.ViewModels.SettingsFlyouts
{
    public class ConversationSettingsFlyoutViewModel
    {
        public ConversationSettingsFlyoutViewModel()
        {
            this.Model = new ConversationSettingsFlyoutModel();

            this.Tokens = this.Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            this.ConversationStatus = this.Model.ToReactivePropertyAsSynchronized(x => x.ConversationStatus);
            this.IconSource = new ReactiveProperty<string>("http://localhost/");

            this.ClearCommand = new ReactiveCommand();
            this.ClearCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                this.Model.Conversation.Clear();
            });

            this.UpdateCommand = new ReactiveCommand();
            this.UpdateCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await this.Model.UpdateConversation();
            });

            this.Conversation = this.Model.Conversation.ToReadOnlyReactiveCollection(x => new StatusViewModel(x, this.Tokens.Value.UserId));

            this.Updating = this.Model.ObserveProperty(x => x.Updating).ToReactiveProperty();

            this.Notice = Services.Notice.Instance;
        }

        public ConversationSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<bool> Updating { get; set; }

        public ReactiveProperty<Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReactiveProperty<Status> ConversationStatus { get; set; }

        public ReadOnlyReactiveCollection<StatusViewModel> Conversation { get; private set; }

        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand UpdateCommand { get; set; }

        public Services.Notice Notice { get; set; }
    }
}
