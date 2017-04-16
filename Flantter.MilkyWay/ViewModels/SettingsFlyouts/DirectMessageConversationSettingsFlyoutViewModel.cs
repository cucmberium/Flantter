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
    public class DirectMessageConversationSettingsFlyoutViewModel
    {
        public DirectMessageConversationSettingsFlyoutViewModel()
        {
            this.Model = new DirectMessageConversationSettingsFlyoutModel();

            this.Tokens = this.Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            this.ScreenName = this.Model.ToReactivePropertyAsSynchronized(x => x.ScreenName);
            this.IconSource = new ReactiveProperty<string>("http://localhost/");

            this.Text = this.Model.ToReactivePropertyAsSynchronized(x => x.Text);

            this.ClearCommand = new ReactiveCommand();
            this.ClearCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                this.Model.DirectMessages.Clear();
            });

            this.UpdateCommand = new ReactiveCommand();
            this.UpdateCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await this.Model.UpdateDirectMessageConversation();
            });

            this.SendCommand = new ReactiveCommand();
            this.SendCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await this.Model.SendDirectMessage();
            });

            this.RefreshCommand = new ReactiveCommand();
            this.RefreshCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await this.Model.UpdateDirectMessageConversation(clear: false);
            });

            this.DirectMessagesIncrementalLoadCommand = new ReactiveCommand();
            this.DirectMessagesIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                if (this.Model.DirectMessages.Count > 0)
                    await this.Model.UpdateDirectMessageConversation(this.Model.DirectMessages.LastOrDefault().Id);
            });

            this.DirectMessages = this.Model.DirectMessages.ToReadOnlyReactiveCollection(x => new DirectMessageViewModel(x, this.Tokens.Value.UserId));
            
            this.Updating = Observable.CombineLatest(
                                this.Model.ObserveProperty(x => x.UpdatingDirectMessages),
                                this.Model.ObserveProperty(x => x.SendingDirectMessage),
                                (updatingDirectMessages, sendingDirectMessage) =>
                                {
                                    return (updatingDirectMessages || sendingDirectMessage);
                                }).ToReactiveProperty();

            this.SendingDirectMessage = this.Model.ObserveProperty(x => x.SendingDirectMessage).ToReactiveProperty();

            this.Notice = Services.Notice.Instance;
        }

        public DirectMessageConversationSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<bool> Updating { get; set; }

        public ReactiveProperty<bool> SendingDirectMessage { get; set; }

        public ReactiveProperty<Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReactiveProperty<string> ScreenName { get; set; }

        public ReactiveProperty<string> Text { get; set; }

        public ReadOnlyReactiveCollection<DirectMessageViewModel> DirectMessages { get; private set; }

        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand UpdateCommand { get; set; }

        public ReactiveCommand SendCommand { get; set; }

        public ReactiveCommand RefreshCommand { get; set; }

        public ReactiveCommand DirectMessagesIncrementalLoadCommand { get; set; }

        public Services.Notice Notice { get; set; }
    }
}
