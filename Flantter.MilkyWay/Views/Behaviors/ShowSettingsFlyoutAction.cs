using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.Views.Contents;
using Flantter.MilkyWay.Views.Contents.SettingsFlyouts;
using Flantter.MilkyWay.Views.Contents.SettingsFlyouts.Settings;
using Flantter.MilkyWay.Views.Controls;
using Flantter.MilkyWay.Views.Util;
using Flantter.MilkyWay.Common;
using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Flantter.MilkyWay.Models.Twitter.Wrapper;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class ShowSettingsFlyoutAction : DependencyObject, IAction
    {
        public static ShowSettingsFlyoutAction GetForCurrentView()
        {
            return _main;
        }

        private static ShowSettingsFlyoutAction _main;

        private List<ExtendedSettingsFlyout> _SettingsFlyoutList = null;
        private VideoPreviewPopup _VideoPreviewPopup = null;
        private ImagePreviewPopup _ImagePreviewPopup = null;

        private List<ContentPopup> _PopupList { get; set; }

        public ShowSettingsFlyoutAction()
        {
            _main = this;

            this._SettingsFlyoutList = new List<ExtendedSettingsFlyout>();
            this._PopupList = new List<ContentPopup>();

            
        }

        public int ShowingPopupCount
        {
            get
            {
                this._PopupList = this._PopupList.Where(x => x.IsOpen).Distinct().ToList();
                return this._PopupList.Count;
            }
        }

        public void HideTopPopup()
        {
            this._PopupList = this._PopupList.Where(x => x.IsOpen).Distinct().ToList();
            if (this._PopupList.Count == 0)
                return;

            var popup = this._PopupList.First();
            popup.Hide();
        }

        public object Execute(object sender, object parameter)
        {
            var notification = parameter as ShowSettingsFlyoutNotification;
            if (notification == null)
                return null;
            
            ExtendedSettingsFlyout settingsFlyout = null;
            IEnumerable<ExtendedSettingsFlyout> settingsFlyoutList = null;

            switch (notification.SettingsFlyoutType)
            {
                case "ImagePreview":
                    var mediaEntity = notification.Content as Models.Twitter.Objects.MediaEntity;
                    if (mediaEntity.ParentEntities == null)
                        return null;

                    if (this._ImagePreviewPopup == null)
                        this._ImagePreviewPopup = new ImagePreviewPopup();

                    this._ImagePreviewPopup.Images = mediaEntity.ParentEntities.Media.Where(x => x.Type == "Image").ToList();
                    this._ImagePreviewPopup.ImageIndex = this._ImagePreviewPopup.Images.IndexOf(mediaEntity);

                    this._ImagePreviewPopup.ImageRefresh();

                    this._ImagePreviewPopup.Show();

                    this._PopupList.Insert(0, this._ImagePreviewPopup);
                    break;

                case "VideoPreview":
                    var videoEntity = notification.Content as Models.Twitter.Objects.MediaEntity;

                    if (_VideoPreviewPopup == null)
                        _VideoPreviewPopup = new VideoPreviewPopup();

                    this._VideoPreviewPopup.Id = videoEntity.VideoInfo.VideoId;
                    this._VideoPreviewPopup.VideoWebUrl = videoEntity.ExpandedUrl;
                    this._VideoPreviewPopup.VideoThumbnailUrl = videoEntity.MediaThumbnailUrl;
                    this._VideoPreviewPopup.VideoType = videoEntity.VideoInfo.VideoType;
                    this._VideoPreviewPopup.VideoContentType = videoEntity.VideoInfo.VideoContentType;
                    this._VideoPreviewPopup.VideoChanged();

                    this._VideoPreviewPopup.Show();
                    this._PopupList.Insert(0, this._VideoPreviewPopup);
                    break;

                case "Search":
                    if (_SettingsFlyoutList.Where(x => x.IsOpen).Count() > 0)
                        break;

                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is SearchSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new SearchSettingsFlyout();
                        ((SearchSettingsFlyout)settingsFlyout).ViewModel = new ViewModels.SettingsFlyouts.SearchSettingsFlyoutViewModel();
                        ((SearchSettingsFlyout)settingsFlyout).DataContext = ((SearchSettingsFlyout)settingsFlyout).ViewModel;
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }

                    ((SearchSettingsFlyout)settingsFlyout).ViewModel.IconSource.Value = notification.UserIcon;
                    ((SearchSettingsFlyout)settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((SearchSettingsFlyout)settingsFlyout).ViewModel.ClearCommand.Execute();

                    ((SearchSettingsFlyout)settingsFlyout).ViewModel.StatusSearchWords.Value = notification.Content as string;

                    settingsFlyout.Show();

                    if (!string.IsNullOrWhiteSpace(notification.Content as string))
                        ((SearchSettingsFlyout)settingsFlyout).ViewModel.UpdateStatusSearchCommand.Execute();
                    else
                        ((SearchSettingsFlyout)settingsFlyout).FocusToStatusSearchBox();

                    this._PopupList.Insert(0, settingsFlyout);
                    break;
                case "UserProfile":
                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is UserProfileSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new UserProfileSettingsFlyout();
                        ((UserProfileSettingsFlyout)settingsFlyout).ViewModel = new ViewModels.SettingsFlyouts.UserProfileSettingsFlyoutViewModel();
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }

                    ((UserProfileSettingsFlyout)settingsFlyout).ViewModel.IconSource.Value = notification.UserIcon;
                    ((UserProfileSettingsFlyout)settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((UserProfileSettingsFlyout)settingsFlyout).ViewModel.ClearCommand.Execute();
                    
                    ((UserProfileSettingsFlyout)settingsFlyout).ViewModel.ScreenName.Value = notification.Content as string;

                    ((UserProfileSettingsFlyout)settingsFlyout).ViewModel.UpdateCommand.Execute();

                    settingsFlyout.Show();

                    this._PopupList.Insert(0, settingsFlyout);
                    break;
                case "Conversation":
                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is ConversationSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new ConversationSettingsFlyout();
                        ((ConversationSettingsFlyout)settingsFlyout).ViewModel = new ViewModels.SettingsFlyouts.ConversationSettingsFlyoutViewModel();
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }

                    ((ConversationSettingsFlyout)settingsFlyout).ViewModel.IconSource.Value = notification.UserIcon;
                    ((ConversationSettingsFlyout)settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((ConversationSettingsFlyout)settingsFlyout).ViewModel.ClearCommand.Execute();
                    
                    ((ConversationSettingsFlyout)settingsFlyout).ViewModel.ConversationStatus.Value = notification.Content as Models.Twitter.Objects.Status;

                    ((ConversationSettingsFlyout)settingsFlyout).ViewModel.UpdateCommand.Execute();

                    settingsFlyout.Show();

                    this._PopupList.Insert(0, settingsFlyout);
                    break;
                case "StatusDetail":
                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is StatusDetailSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new StatusDetailSettingsFlyout();
                        ((StatusDetailSettingsFlyout)settingsFlyout).ViewModel = new ViewModels.SettingsFlyouts.StatusDetailSettingsFlyoutViewModel();
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }

                    ((StatusDetailSettingsFlyout)settingsFlyout).ViewModel.IconSource.Value = notification.UserIcon;
                    ((StatusDetailSettingsFlyout)settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((StatusDetailSettingsFlyout)settingsFlyout).ViewModel.ClearCommand.Execute();

                    if (notification.Content is Models.Twitter.Objects.Status)
                    {
                        var status = notification.Content as Models.Twitter.Objects.Status;
                        ((StatusDetailSettingsFlyout)settingsFlyout).ViewModel.Model.Status = status;
                        ((StatusDetailSettingsFlyout)settingsFlyout).ViewModel.StatusId.Value = status.Id;
                    }
                    else if (notification.Content is long)
                    {
                        var statusId = (long)notification.Content;
                        ((StatusDetailSettingsFlyout)settingsFlyout).ViewModel.StatusId.Value = statusId;
                        ((StatusDetailSettingsFlyout)settingsFlyout).ViewModel.UpdateStatusCommand.Execute();
                    }

                    settingsFlyout.Show();

                    this._PopupList.Insert(0, settingsFlyout);
                    break;
                case "DirectMessageConversation":
                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is DirectMessageConversationSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new DirectMessageConversationSettingsFlyout();
                        ((DirectMessageConversationSettingsFlyout)settingsFlyout).ViewModel = new ViewModels.SettingsFlyouts.DirectMessageConversationSettingsFlyoutViewModel();
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }

                    ((DirectMessageConversationSettingsFlyout)settingsFlyout).ViewModel.IconSource.Value = notification.UserIcon;
                    ((DirectMessageConversationSettingsFlyout)settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((DirectMessageConversationSettingsFlyout)settingsFlyout).ViewModel.ClearCommand.Execute();

                    ((DirectMessageConversationSettingsFlyout)settingsFlyout).ViewModel.ScreenName.Value = notification.Content as string;

                    ((DirectMessageConversationSettingsFlyout)settingsFlyout).ViewModel.UpdateCommand.Execute();

                    ((DirectMessageConversationSettingsFlyout)settingsFlyout).DataContext = ((DirectMessageConversationSettingsFlyout)settingsFlyout).ViewModel;

                    settingsFlyout.Show();

                    this._PopupList.Insert(0, settingsFlyout);
                    break;
                case "UserLists":
                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is UserListsSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new UserListsSettingsFlyout();
                        ((UserListsSettingsFlyout)settingsFlyout).ViewModel = new ViewModels.SettingsFlyouts.UserListsSettingsFlyoutViewModel();
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }

                    ((UserListsSettingsFlyout)settingsFlyout).ViewModel.IconSource.Value = notification.UserIcon;
                    ((UserListsSettingsFlyout)settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((UserListsSettingsFlyout)settingsFlyout).ViewModel.ClearCommand.Execute();

                    ((UserListsSettingsFlyout)settingsFlyout).ViewModel.ScreenName.Value = notification.Content as string;

                    ((UserListsSettingsFlyout)settingsFlyout).ViewModel.UpdateCommand.Execute();

                    ((UserListsSettingsFlyout)settingsFlyout).DataContext = ((UserListsSettingsFlyout)settingsFlyout).ViewModel;

                    settingsFlyout.Show();

                    this._PopupList.Insert(0, settingsFlyout);
                    break;
                case "UserCollections":
                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is UserCollectionsSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new UserCollectionsSettingsFlyout();
                        ((UserCollectionsSettingsFlyout)settingsFlyout).ViewModel = new ViewModels.SettingsFlyouts.UserCollectionsSettingsFlyoutViewModel();
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }

                    ((UserCollectionsSettingsFlyout)settingsFlyout).ViewModel.IconSource.Value = notification.UserIcon;
                    ((UserCollectionsSettingsFlyout)settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((UserCollectionsSettingsFlyout)settingsFlyout).ViewModel.ScreenName.Value = notification.Content as string;

                    ((UserCollectionsSettingsFlyout)settingsFlyout).ViewModel.ClearCommand.Execute();
                    
                    ((UserCollectionsSettingsFlyout)settingsFlyout).ViewModel.UpdateCommand.Execute();

                    ((UserCollectionsSettingsFlyout)settingsFlyout).DataContext = ((UserCollectionsSettingsFlyout)settingsFlyout).ViewModel;

                    settingsFlyout.Show();

                    this._PopupList.Insert(0, settingsFlyout);
                    break;
                case "ListStatuses":
                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is ListStatusesSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new ListStatusesSettingsFlyout();
                        ((ListStatusesSettingsFlyout)settingsFlyout).ViewModel = new ViewModels.SettingsFlyouts.ListStatusesSettingsFlyoutViewModel();
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }

                    ((ListStatusesSettingsFlyout)settingsFlyout).ViewModel.IconSource.Value = notification.UserIcon;
                    ((ListStatusesSettingsFlyout)settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((ListStatusesSettingsFlyout)settingsFlyout).ViewModel.ClearCommand.Execute();

                    ((ListStatusesSettingsFlyout)settingsFlyout).ViewModel.Id.Value = ((Models.Twitter.Objects.List)notification.Content).Id;
                    ((ListStatusesSettingsFlyout)settingsFlyout).ViewModel.FullName.Value = ((Models.Twitter.Objects.List)notification.Content).FullName;

                    ((ListStatusesSettingsFlyout)settingsFlyout).ViewModel.UpdateCommand.Execute();

                    ((ListStatusesSettingsFlyout)settingsFlyout).DataContext = ((ListStatusesSettingsFlyout)settingsFlyout).ViewModel;

                    settingsFlyout.Show();

                    this._PopupList.Insert(0, settingsFlyout);
                    break;
                case "ListMembers":
                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is ListMembersSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new ListMembersSettingsFlyout();
                        ((ListMembersSettingsFlyout)settingsFlyout).ViewModel = new ViewModels.SettingsFlyouts.ListMembersSettingsFlyoutViewModel();
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }

                    ((ListMembersSettingsFlyout)settingsFlyout).ViewModel.IconSource.Value = notification.UserIcon;
                    ((ListMembersSettingsFlyout)settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((ListMembersSettingsFlyout)settingsFlyout).ViewModel.ClearCommand.Execute();

                    ((ListMembersSettingsFlyout)settingsFlyout).ViewModel.Id.Value = ((Models.Twitter.Objects.List)notification.Content).Id;

                    ((ListMembersSettingsFlyout)settingsFlyout).ViewModel.UpdateCommand.Execute();

                    ((ListMembersSettingsFlyout)settingsFlyout).DataContext = ((ListMembersSettingsFlyout)settingsFlyout).ViewModel;

                    settingsFlyout.Show();

                    this._PopupList.Insert(0, settingsFlyout);
                    break;
                case "CollectionStatuses":
                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is CollectionStatusesSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new CollectionStatusesSettingsFlyout();
                        ((CollectionStatusesSettingsFlyout)settingsFlyout).ViewModel = new ViewModels.SettingsFlyouts.CollectionStatusesSettingsFlyoutViewModel();
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }

                    ((CollectionStatusesSettingsFlyout)settingsFlyout).ViewModel.IconSource.Value = notification.UserIcon;
                    ((CollectionStatusesSettingsFlyout)settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((CollectionStatusesSettingsFlyout)settingsFlyout).ViewModel.ClearCommand.Execute();

                    ((CollectionStatusesSettingsFlyout)settingsFlyout).ViewModel.Id.Value = ((Models.Twitter.Objects.Collection)notification.Content).Id;
                    ((CollectionStatusesSettingsFlyout)settingsFlyout).ViewModel.Name.Value = ((Models.Twitter.Objects.Collection)notification.Content).Name + " - @" + ((Models.Twitter.Objects.Collection)notification.Content).User.ScreenName;

                    ((CollectionStatusesSettingsFlyout)settingsFlyout).ViewModel.UpdateCommand.Execute();

                    ((CollectionStatusesSettingsFlyout)settingsFlyout).DataContext = ((CollectionStatusesSettingsFlyout)settingsFlyout).ViewModel;

                    settingsFlyout.Show();

                    this._PopupList.Insert(0, settingsFlyout);
                    break;
                case "Retweeters":
                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is RetweetersSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new RetweetersSettingsFlyout();
                        ((RetweetersSettingsFlyout)settingsFlyout).ViewModel = new ViewModels.SettingsFlyouts.RetweetersSettingsFlyoutViewModel();
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }

                    ((RetweetersSettingsFlyout)settingsFlyout).ViewModel.IconSource.Value = notification.UserIcon;
                    ((RetweetersSettingsFlyout)settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((RetweetersSettingsFlyout)settingsFlyout).ViewModel.ClearCommand.Execute();
                    
                    ((RetweetersSettingsFlyout)settingsFlyout).ViewModel.Id.Value = ((Models.Twitter.Objects.Status)notification.Content).Id;

                    ((RetweetersSettingsFlyout)settingsFlyout).ViewModel.UpdateCommand.Execute();

                    ((RetweetersSettingsFlyout)settingsFlyout).DataContext = ((RetweetersSettingsFlyout)settingsFlyout).ViewModel;

                    settingsFlyout.Show();

                    this._PopupList.Insert(0, settingsFlyout);
                    break;
                case "RetweetsOfMe":
                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is RetweetsOfMeSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new RetweetsOfMeSettingsFlyout();
                        ((RetweetsOfMeSettingsFlyout)settingsFlyout).ViewModel = new ViewModels.SettingsFlyouts.RetweetsOfMeSettingsFlyoutViewModel();
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }

                    ((RetweetsOfMeSettingsFlyout)settingsFlyout).ViewModel.IconSource.Value = notification.UserIcon;
                    ((RetweetsOfMeSettingsFlyout)settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((RetweetsOfMeSettingsFlyout)settingsFlyout).ViewModel.ClearCommand.Execute();
                    ((RetweetsOfMeSettingsFlyout)settingsFlyout).ViewModel.UpdateCommand.Execute();

                    ((RetweetsOfMeSettingsFlyout)settingsFlyout).DataContext = ((RetweetsOfMeSettingsFlyout)settingsFlyout).ViewModel;

                    settingsFlyout.Show();

                    this._PopupList.Insert(0, settingsFlyout);
                    break;
                case "UserFollowInfo":
                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is UserFollowInfoSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new UserFollowInfoSettingsFlyout();
                        ((UserFollowInfoSettingsFlyout)settingsFlyout).ViewModel = new ViewModels.SettingsFlyouts.UserFollowInfoSettingsFlyoutViewModel();
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }

                    ((UserFollowInfoSettingsFlyout)settingsFlyout).ViewModel.IconSource.Value = notification.UserIcon;
                    ((UserFollowInfoSettingsFlyout)settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((UserFollowInfoSettingsFlyout)settingsFlyout).ViewModel.ClearCommand.Execute();
                    ((UserFollowInfoSettingsFlyout)settingsFlyout).ViewModel.UpdateCommand.Execute();

                    ((UserFollowInfoSettingsFlyout)settingsFlyout).DataContext = ((UserFollowInfoSettingsFlyout)settingsFlyout).ViewModel;

                    settingsFlyout.Show();

                    this._PopupList.Insert(0, settingsFlyout);
                    break;
                case "AddToCollection":
                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is AddStatusToCollectionSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new AddStatusToCollectionSettingsFlyout();
                        ((AddStatusToCollectionSettingsFlyout)settingsFlyout).ViewModel = new ViewModels.SettingsFlyouts.AddStatusToCollectionSettingsFlyoutViewModel();
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }

                    ((AddStatusToCollectionSettingsFlyout)settingsFlyout).ViewModel.IconSource.Value = notification.UserIcon;
                    ((AddStatusToCollectionSettingsFlyout)settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((AddStatusToCollectionSettingsFlyout)settingsFlyout).ViewModel.ClearCommand.Execute();

                    ((AddStatusToCollectionSettingsFlyout)settingsFlyout).ViewModel.ScreenName.Value = notification.Tokens.ScreenName;
                    ((AddStatusToCollectionSettingsFlyout)settingsFlyout).ViewModel.Status.Value = ((Models.Twitter.Objects.Status)notification.Content);

                    ((AddStatusToCollectionSettingsFlyout)settingsFlyout).ViewModel.UpdateCommand.Execute();
                    
                    ((AddStatusToCollectionSettingsFlyout)settingsFlyout).DataContext = ((AddStatusToCollectionSettingsFlyout)settingsFlyout).ViewModel;

                    settingsFlyout.Show();

                    this._PopupList.Insert(0, settingsFlyout);
                    break;
                case "MainSetting":
                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is MainSettingSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new MainSettingSettingsFlyout();
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }
                    
                    settingsFlyout.Show();

                    this._PopupList.Insert(0, settingsFlyout);
                    break;
                case "BehaviorSetting":
                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is BehaviorSettingSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new BehaviorSettingSettingsFlyout();
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }

                    settingsFlyout.Show();

                    this._PopupList.Insert(0, settingsFlyout);
                    break;
                case "PostingSetting":
                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is PostingSettingSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new PostingSettingSettingsFlyout();
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }

                    settingsFlyout.Show();

                    this._PopupList.Insert(0, settingsFlyout);
                    break;
                case "DisplaySetting":
                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is DisplaySettingSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new DisplaySettingSettingsFlyout();
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }

                    settingsFlyout.Show();

                    this._PopupList.Insert(0, settingsFlyout);
                    break;
                case "NotificationSetting":
                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is NotificationSettingSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new NotificationSettingSettingsFlyout();
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }

                    settingsFlyout.Show();

                    this._PopupList.Insert(0, settingsFlyout);
                    break;
                case "MuteSetting":
                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is MuteSettingSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new MuteSettingSettingsFlyout();
                        ((MuteSettingSettingsFlyout)settingsFlyout).ViewModel = new ViewModels.SettingsFlyouts.Settings.MuteSettingSettingsFlyoutViewModel();
                        settingsFlyout.DataContext = ((MuteSettingSettingsFlyout)settingsFlyout).ViewModel;
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }

                    ((MuteSettingSettingsFlyout)settingsFlyout).ViewModel.MuteFilter.Value = Setting.SettingService.Setting.MuteFilter;
                    settingsFlyout.Show();

                    this._PopupList.Insert(0, settingsFlyout);
                    break;
                case "DatabaseSetting":
                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is DatabaseSettingSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new DatabaseSettingSettingsFlyout();
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }

                    settingsFlyout.Show();

                    this._PopupList.Insert(0, settingsFlyout);
                    break;
                case "AccountsSetting":
                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is AccountsSettingSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new AccountsSettingSettingsFlyout();
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }

                    settingsFlyout.Show();

                    this._PopupList.Insert(0, settingsFlyout);
                    break;
                case "AccountSetting":
                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is AccountSettingSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new AccountSettingSettingsFlyout();
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }

                    ((AccountSettingSettingsFlyout)settingsFlyout).ViewModel = notification.Content as AccountSetting;

                    settingsFlyout.Show();

                    this._PopupList.Insert(0, settingsFlyout);
                    break;
                case "AdvancedSetting":
                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is AdvancedSettingSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new AdvancedSettingSettingsFlyout();
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }

                    settingsFlyout.Show();

                    this._PopupList.Insert(0, settingsFlyout);
                    break;
                case "ColumnSetting":
                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is ColumnSettingSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new ColumnSettingSettingsFlyout();
                        ((ColumnSettingSettingsFlyout)settingsFlyout).ViewModel = new ViewModels.SettingsFlyouts.Settings.ColumnSettingSettingsFlyoutViewModel();
                        settingsFlyout.DataContext = ((ColumnSettingSettingsFlyout)settingsFlyout).ViewModel;
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }

                    ((ColumnSettingSettingsFlyout)settingsFlyout).ViewModel.ColumnSetting.Value = notification.Content as ColumnSetting;

                    settingsFlyout.Show();

                    this._PopupList.Insert(0, settingsFlyout);
                    break;
                case "AppInfo":
                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is AppInfoSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new AppInfoSettingsFlyout();
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }

                    settingsFlyout.Show();

                    this._PopupList.Insert(0, settingsFlyout);
                    break;
                case "AccountChange":
                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is AccountChangeSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new AccountChangeSettingsFlyout();
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }

                    settingsFlyout.Show();

                    this._PopupList.Insert(0, settingsFlyout);
                    break;
            }

            this._PopupList = this._PopupList.Where(x => x.IsOpen).Distinct().ToList();

            return null;
        }
    }
    public class ShowSettingsFlyoutNotification : Notification
    {
        /// <summary>
        /// SettingsFlyoutの種類
        /// </summary>
        public string SettingsFlyoutType { get; set; }

        /// <summary>
        /// WrapperのTokens
        /// </summary>
        public Tokens Tokens { get; set; }

        /// <summary>
        /// ユーザーのアイコン
        /// </summary>
        public string UserIcon { get; set; }
    }

}
