﻿using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Flantter.MilkyWay.Models.Apis.Wrapper;
using Flantter.MilkyWay.Models.SettingsFlyouts;
using Flantter.MilkyWay.ViewModels.Apis.Objects;
using Flantter.MilkyWay.ViewModels.Services;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Flantter.MilkyWay.ViewModels.SettingsFlyouts
{
    public class DirectMessageConversationSettingsFlyoutViewModel
    {
        public DirectMessageConversationSettingsFlyoutViewModel()
        {
            Model = new DirectMessageConversationSettingsFlyoutModel();

            Tokens = Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            IconSource = new ReactiveProperty<string>("http://localhost/");
            UserId = Model.ToReactivePropertyAsSynchronized(x => x.UserId);

            Text = Model.ToReactivePropertyAsSynchronized(x => x.Text);

            ScreenName = Model.ObserveProperty(x => x.ScreenName).ToReactiveProperty();

            ClearCommand = new ReactiveCommand();
            ClearCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                Model.UserId = 0;

                Model.DirectMessages.Clear();
            });

            UpdateCommand = new ReactiveCommand();
            UpdateCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    await Model.UpdateUserInfomation();
                    await Model.UpdateDirectMessageConversation();
                });

            SendCommand = new ReactiveCommand();
            SendCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.SendDirectMessage(); });

            RefreshCommand = new ReactiveCommand();
            RefreshCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.UpdateDirectMessageConversation(clear: false); });

            DirectMessagesIncrementalLoadCommand = new ReactiveCommand();
            DirectMessagesIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (Model.DirectMessages.Count > 0)
                        await Model.UpdateDirectMessageConversation(Model.DirectMessages.LastOrDefault().Id);
                });

            DirectMessages =
                Model.DirectMessages.ToReadOnlyReactiveCollection(
                    x => new DirectMessageViewModel(x, Tokens.Value.UserId));

            Updating = Model.ObserveProperty(x => x.UpdatingDirectMessages)
                .CombineLatest(Model.ObserveProperty(x => x.SendingDirectMessage),
                    (updatingDirectMessages, sendingDirectMessage) =>
                    {
                        return updatingDirectMessages || sendingDirectMessage;
                    })
                .ToReactiveProperty();

            SendingDirectMessage = Model.ObserveProperty(x => x.SendingDirectMessage).ToReactiveProperty();

            Notice = Notice.Instance;
        }

        public DirectMessageConversationSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<bool> Updating { get; set; }

        public ReactiveProperty<bool> SendingDirectMessage { get; set; }

        public ReactiveProperty<Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReactiveProperty<long> UserId { get; set; }

        public ReactiveProperty<string> ScreenName { get; set; }

        public ReactiveProperty<string> Text { get; set; }

        public ReadOnlyReactiveCollection<DirectMessageViewModel> DirectMessages { get; }

        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand UpdateCommand { get; set; }

        public ReactiveCommand SendCommand { get; set; }

        public ReactiveCommand RefreshCommand { get; set; }

        public ReactiveCommand DirectMessagesIncrementalLoadCommand { get; set; }

        public Notice Notice { get; set; }
    }
}