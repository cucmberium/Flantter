using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Models.Twitter.Wrapper;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.SettingsFlyouts;
using Flantter.MilkyWay.ViewModels.SettingsFlyouts.Settings;
using Flantter.MilkyWay.Views.Contents;
using Flantter.MilkyWay.Views.Contents.SettingsFlyouts;
using Flantter.MilkyWay.Views.Contents.SettingsFlyouts.Settings;
using Flantter.MilkyWay.Views.Controls;
using Flantter.MilkyWay.Views.Util;
using Microsoft.Xaml.Interactivity;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class ShowSettingsFlyoutAction : DependencyObject, IAction
    {
        private static ShowSettingsFlyoutAction _main;

        private ImagePreviewPopup _imagePreviewPopup;
        private readonly List<ExtendedSettingsFlyout> _settingsFlyoutList;
        private VideoPreviewPopup _videoPreviewPopup;

        public ShowSettingsFlyoutAction()
        {
            _main = this;

            _settingsFlyoutList = new List<ExtendedSettingsFlyout>();
            PopupList = new List<IContentPopup>();
        }

        private List<IContentPopup> PopupList { get; set; }

        public int ShowingPopupCount
        {
            get
            {
                PopupList = PopupList.Where(x => x.IsOpen).Distinct().ToList();
                return PopupList.Count;
            }
        }

        public object Execute(object sender, object parameter)
        {
            var notification = parameter as ShowSettingsFlyoutNotification;
            if (notification == null)
                return null;

            ExtendedSettingsFlyout settingsFlyout;
            IEnumerable<ExtendedSettingsFlyout> settingsFlyoutList;

            switch (notification.SettingsFlyoutType)
            {
                case "ImagePreview":
                    var mediaEntity = notification.Content as MediaEntity;
                    if (mediaEntity?.ParentEntities == null)
                        return null;

                    if (_imagePreviewPopup == null)
                        _imagePreviewPopup = new ImagePreviewPopup();

                    _imagePreviewPopup.Images = mediaEntity.ParentEntities.Media.Where(x => x.Type == "Image").ToList();
                    _imagePreviewPopup.ImageIndex = _imagePreviewPopup.Images.IndexOf(mediaEntity);

                    _imagePreviewPopup.ImageRefresh();

                    _imagePreviewPopup.Show();

                    PopupList.Insert(0, _imagePreviewPopup);
                    break;

                case "VideoPreview":
                    var videoEntity = notification.Content as MediaEntity;

                    if (_videoPreviewPopup == null)
                        _videoPreviewPopup = new VideoPreviewPopup();

                    _videoPreviewPopup.Id = videoEntity.VideoInfo.VideoId;
                    _videoPreviewPopup.VideoWebUrl = videoEntity.ExpandedUrl;
                    _videoPreviewPopup.VideoThumbnailUrl = videoEntity.MediaThumbnailUrl;
                    _videoPreviewPopup.VideoType = videoEntity.VideoInfo.VideoType;
                    _videoPreviewPopup.VideoContentType = videoEntity.VideoInfo.VideoContentType;
                    _videoPreviewPopup.VideoChanged();

                    _videoPreviewPopup.Show();
                    PopupList.Insert(0, _videoPreviewPopup);
                    break;

                case "Search":
                    if (_settingsFlyoutList.Count(x => x.IsOpen) > 0)
                        break;

                    settingsFlyoutList = _settingsFlyoutList.Where(x => x is SearchSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new SearchSettingsFlyout();
                        ((SearchSettingsFlyout) settingsFlyout).ViewModel = new SearchSettingsFlyoutViewModel();
                        ((SearchSettingsFlyout) settingsFlyout).DataContext =
                            ((SearchSettingsFlyout) settingsFlyout).ViewModel;
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    ((SearchSettingsFlyout) settingsFlyout).ViewModel.IconSource.Value = notification.UserIcon;
                    ((SearchSettingsFlyout) settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((SearchSettingsFlyout) settingsFlyout).ViewModel.ClearCommand.Execute();

                    ((SearchSettingsFlyout) settingsFlyout).ViewModel.StatusSearchWords.Value =
                        notification.Content as string;

                    settingsFlyout.Show();

                    if (!string.IsNullOrWhiteSpace(notification.Content as string))
                        ((SearchSettingsFlyout) settingsFlyout).ViewModel.UpdateStatusSearchCommand.Execute();
                    else
                        ((SearchSettingsFlyout) settingsFlyout).FocusToStatusSearchBox();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "UserProfile":
                    settingsFlyoutList = _settingsFlyoutList.Where(x => x is UserProfileSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new UserProfileSettingsFlyout();
                        ((UserProfileSettingsFlyout) settingsFlyout).ViewModel =
                            new UserProfileSettingsFlyoutViewModel();
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    ((UserProfileSettingsFlyout) settingsFlyout).ViewModel.IconSource.Value = notification.UserIcon;
                    ((UserProfileSettingsFlyout) settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((UserProfileSettingsFlyout) settingsFlyout).ViewModel.ClearCommand.Execute();

                    if (notification.Content is long)
                        ((UserProfileSettingsFlyout) settingsFlyout).ViewModel.UserId.Value =
                            (long)notification.Content;
                    else
                        ((UserProfileSettingsFlyout)settingsFlyout).ViewModel.ScreenName.Value =
                            notification.Content as string;

                    ((UserProfileSettingsFlyout) settingsFlyout).ViewModel.UpdateCommand.Execute();

                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "Conversation":
                    settingsFlyoutList = _settingsFlyoutList.Where(x => x is ConversationSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new ConversationSettingsFlyout();
                        ((ConversationSettingsFlyout) settingsFlyout).ViewModel =
                            new ConversationSettingsFlyoutViewModel();
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    ((ConversationSettingsFlyout) settingsFlyout).ViewModel.IconSource.Value = notification.UserIcon;
                    ((ConversationSettingsFlyout) settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((ConversationSettingsFlyout) settingsFlyout).ViewModel.ClearCommand.Execute();

                    ((ConversationSettingsFlyout) settingsFlyout).ViewModel.ConversationStatus.Value =
                        notification.Content as Status;

                    ((ConversationSettingsFlyout) settingsFlyout).ViewModel.UpdateCommand.Execute();

                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "StatusDetail":
                    settingsFlyoutList = _settingsFlyoutList.Where(x => x is StatusDetailSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new StatusDetailSettingsFlyout();
                        ((StatusDetailSettingsFlyout) settingsFlyout).ViewModel =
                            new StatusDetailSettingsFlyoutViewModel();
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    ((StatusDetailSettingsFlyout) settingsFlyout).ViewModel.IconSource.Value = notification.UserIcon;
                    ((StatusDetailSettingsFlyout) settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((StatusDetailSettingsFlyout) settingsFlyout).ViewModel.ClearCommand.Execute();

                    if (notification.Content is Status)
                    {
                        var status = notification.Content as Status;
                        ((StatusDetailSettingsFlyout) settingsFlyout).ViewModel.Model.Status = status;
                        ((StatusDetailSettingsFlyout) settingsFlyout).ViewModel.StatusId.Value = status.Id;
                    }
                    else if (notification.Content is long)
                    {
                        var statusId = (long) notification.Content;
                        ((StatusDetailSettingsFlyout) settingsFlyout).ViewModel.StatusId.Value = statusId;
                        ((StatusDetailSettingsFlyout) settingsFlyout).ViewModel.UpdateStatusCommand.Execute();
                    }

                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "DirectMessageConversation":
                    settingsFlyoutList =
                        _settingsFlyoutList.Where(x => x is DirectMessageConversationSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new DirectMessageConversationSettingsFlyout();
                        ((DirectMessageConversationSettingsFlyout) settingsFlyout).ViewModel =
                            new DirectMessageConversationSettingsFlyoutViewModel();
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    ((DirectMessageConversationSettingsFlyout) settingsFlyout).ViewModel.IconSource.Value =
                        notification.UserIcon;
                    ((DirectMessageConversationSettingsFlyout) settingsFlyout).ViewModel.Tokens.Value =
                        notification.Tokens;

                    ((DirectMessageConversationSettingsFlyout) settingsFlyout).ViewModel.ClearCommand.Execute();

                    ((DirectMessageConversationSettingsFlyout)settingsFlyout).ViewModel.UserId.Value = (long)notification.Content;
                    
                    ((DirectMessageConversationSettingsFlyout) settingsFlyout).ViewModel.UpdateCommand.Execute();

                    ((DirectMessageConversationSettingsFlyout) settingsFlyout).DataContext =
                        ((DirectMessageConversationSettingsFlyout) settingsFlyout).ViewModel;

                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "UserLists":
                    settingsFlyoutList = _settingsFlyoutList.Where(x => x is UserListsSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new UserListsSettingsFlyout();
                        ((UserListsSettingsFlyout) settingsFlyout).ViewModel = new UserListsSettingsFlyoutViewModel();
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    ((UserListsSettingsFlyout) settingsFlyout).ViewModel.IconSource.Value = notification.UserIcon;
                    ((UserListsSettingsFlyout) settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((UserListsSettingsFlyout) settingsFlyout).ViewModel.ClearCommand.Execute();

                    ((UserListsSettingsFlyout) settingsFlyout).ViewModel.UserId.Value =
                        (long)notification.Content;

                    ((UserListsSettingsFlyout) settingsFlyout).ViewModel.UpdateCommand.Execute();

                    ((UserListsSettingsFlyout) settingsFlyout).DataContext =
                        ((UserListsSettingsFlyout) settingsFlyout).ViewModel;

                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "UserCollections":
                    settingsFlyoutList =
                        _settingsFlyoutList.Where(x => x is UserCollectionsSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new UserCollectionsSettingsFlyout();
                        ((UserCollectionsSettingsFlyout) settingsFlyout).ViewModel =
                            new UserCollectionsSettingsFlyoutViewModel();
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    ((UserCollectionsSettingsFlyout) settingsFlyout).ViewModel.IconSource.Value = notification.UserIcon;
                    ((UserCollectionsSettingsFlyout) settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((UserCollectionsSettingsFlyout) settingsFlyout).ViewModel.UserId.Value =
                        (long)notification.Content;

                    ((UserCollectionsSettingsFlyout) settingsFlyout).ViewModel.ClearCommand.Execute();

                    ((UserCollectionsSettingsFlyout) settingsFlyout).ViewModel.UpdateCommand.Execute();

                    ((UserCollectionsSettingsFlyout) settingsFlyout).DataContext =
                        ((UserCollectionsSettingsFlyout) settingsFlyout).ViewModel;

                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "UserMediaStatuses":
                    settingsFlyoutList =
                        _settingsFlyoutList.Where(x => x is UserMediaStatusesSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new UserMediaStatusesSettingsFlyout();
                        ((UserMediaStatusesSettingsFlyout)settingsFlyout).ViewModel =
                            new UserMediaStatusesSettingsFlyoutViewModel();
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    ((UserMediaStatusesSettingsFlyout)settingsFlyout).ViewModel.IconSource.Value = notification.UserIcon;
                    ((UserMediaStatusesSettingsFlyout)settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((UserMediaStatusesSettingsFlyout)settingsFlyout).ViewModel.UserId.Value =
                        (long)notification.Content;

                    ((UserMediaStatusesSettingsFlyout)settingsFlyout).ViewModel.ClearCommand.Execute();

                    ((UserMediaStatusesSettingsFlyout)settingsFlyout).ViewModel.UpdateCommand.Execute();

                    ((UserMediaStatusesSettingsFlyout)settingsFlyout).DataContext =
                        ((UserMediaStatusesSettingsFlyout)settingsFlyout).ViewModel;

                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "ListStatuses":
                    settingsFlyoutList = _settingsFlyoutList.Where(x => x is ListStatusesSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new ListStatusesSettingsFlyout();
                        ((ListStatusesSettingsFlyout) settingsFlyout).ViewModel =
                            new ListStatusesSettingsFlyoutViewModel();
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    ((ListStatusesSettingsFlyout) settingsFlyout).ViewModel.IconSource.Value = notification.UserIcon;
                    ((ListStatusesSettingsFlyout) settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((ListStatusesSettingsFlyout) settingsFlyout).ViewModel.ClearCommand.Execute();

                    ((ListStatusesSettingsFlyout) settingsFlyout).ViewModel.Id.Value = ((List) notification.Content).Id;
                    ((ListStatusesSettingsFlyout) settingsFlyout).ViewModel.FullName.Value =
                        ((List) notification.Content).FullName;

                    ((ListStatusesSettingsFlyout) settingsFlyout).ViewModel.UpdateCommand.Execute();

                    ((ListStatusesSettingsFlyout) settingsFlyout).DataContext =
                        ((ListStatusesSettingsFlyout) settingsFlyout).ViewModel;

                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "ListMembers":
                    settingsFlyoutList = _settingsFlyoutList.Where(x => x is ListMembersSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new ListMembersSettingsFlyout();
                        ((ListMembersSettingsFlyout) settingsFlyout).ViewModel =
                            new ListMembersSettingsFlyoutViewModel();
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    ((ListMembersSettingsFlyout) settingsFlyout).ViewModel.IconSource.Value = notification.UserIcon;
                    ((ListMembersSettingsFlyout) settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((ListMembersSettingsFlyout) settingsFlyout).ViewModel.ClearCommand.Execute();

                    ((ListMembersSettingsFlyout) settingsFlyout).ViewModel.Id.Value = ((List) notification.Content).Id;

                    ((ListMembersSettingsFlyout) settingsFlyout).ViewModel.UpdateCommand.Execute();

                    ((ListMembersSettingsFlyout) settingsFlyout).DataContext =
                        ((ListMembersSettingsFlyout) settingsFlyout).ViewModel;

                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "CollectionStatuses":
                    settingsFlyoutList =
                        _settingsFlyoutList.Where(x => x is CollectionStatusesSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new CollectionStatusesSettingsFlyout();
                        ((CollectionStatusesSettingsFlyout) settingsFlyout).ViewModel =
                            new CollectionStatusesSettingsFlyoutViewModel();
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    ((CollectionStatusesSettingsFlyout) settingsFlyout).ViewModel.IconSource.Value =
                        notification.UserIcon;
                    ((CollectionStatusesSettingsFlyout) settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((CollectionStatusesSettingsFlyout) settingsFlyout).ViewModel.ClearCommand.Execute();

                    ((CollectionStatusesSettingsFlyout) settingsFlyout).ViewModel.Id.Value =
                        ((Collection) notification.Content).Id;
                    ((CollectionStatusesSettingsFlyout) settingsFlyout).ViewModel.Name.Value =
                        ((Collection) notification.Content).Name + " - @" +
                        ((Collection) notification.Content).User.ScreenName;

                    ((CollectionStatusesSettingsFlyout) settingsFlyout).ViewModel.UpdateCommand.Execute();

                    ((CollectionStatusesSettingsFlyout) settingsFlyout).DataContext =
                        ((CollectionStatusesSettingsFlyout) settingsFlyout).ViewModel;

                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "Retweeters":
                    settingsFlyoutList = _settingsFlyoutList.Where(x => x is RetweetersSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new RetweetersSettingsFlyout();
                        ((RetweetersSettingsFlyout) settingsFlyout).ViewModel = new RetweetersSettingsFlyoutViewModel();
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    ((RetweetersSettingsFlyout) settingsFlyout).ViewModel.IconSource.Value = notification.UserIcon;
                    ((RetweetersSettingsFlyout) settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((RetweetersSettingsFlyout) settingsFlyout).ViewModel.ClearCommand.Execute();

                    ((RetweetersSettingsFlyout) settingsFlyout).ViewModel.Id.Value = ((Status) notification.Content).Id;

                    ((RetweetersSettingsFlyout) settingsFlyout).ViewModel.UpdateCommand.Execute();

                    ((RetweetersSettingsFlyout) settingsFlyout).DataContext =
                        ((RetweetersSettingsFlyout) settingsFlyout).ViewModel;

                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "RetweetsOfMe":
                    settingsFlyoutList = _settingsFlyoutList.Where(x => x is RetweetsOfMeSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new RetweetsOfMeSettingsFlyout();
                        ((RetweetsOfMeSettingsFlyout) settingsFlyout).ViewModel =
                            new RetweetsOfMeSettingsFlyoutViewModel();
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    ((RetweetsOfMeSettingsFlyout) settingsFlyout).ViewModel.IconSource.Value = notification.UserIcon;
                    ((RetweetsOfMeSettingsFlyout) settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((RetweetsOfMeSettingsFlyout) settingsFlyout).ViewModel.ClearCommand.Execute();
                    ((RetweetsOfMeSettingsFlyout) settingsFlyout).ViewModel.UpdateCommand.Execute();

                    ((RetweetsOfMeSettingsFlyout) settingsFlyout).DataContext =
                        ((RetweetsOfMeSettingsFlyout) settingsFlyout).ViewModel;

                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "UserFollowInfo":
                    settingsFlyoutList = _settingsFlyoutList.Where(x => x is UserFollowInfoSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new UserFollowInfoSettingsFlyout();
                        ((UserFollowInfoSettingsFlyout) settingsFlyout).ViewModel =
                            new UserFollowInfoSettingsFlyoutViewModel();
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    ((UserFollowInfoSettingsFlyout) settingsFlyout).ViewModel.IconSource.Value = notification.UserIcon;
                    ((UserFollowInfoSettingsFlyout) settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((UserFollowInfoSettingsFlyout) settingsFlyout).ViewModel.ClearCommand.Execute();
                    ((UserFollowInfoSettingsFlyout) settingsFlyout).ViewModel.UpdateCommand.Execute();

                    ((UserFollowInfoSettingsFlyout) settingsFlyout).DataContext =
                        ((UserFollowInfoSettingsFlyout) settingsFlyout).ViewModel;

                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "PublicTimeline":
                    settingsFlyoutList = _settingsFlyoutList.Where(x => x is PublicTimelineSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new PublicTimelineSettingsFlyout();
                        ((PublicTimelineSettingsFlyout)settingsFlyout).ViewModel =
                            new PublicTimelineSettingsFlyoutViewModel();
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    ((PublicTimelineSettingsFlyout)settingsFlyout).ViewModel.IconSource.Value = notification.UserIcon;
                    ((PublicTimelineSettingsFlyout)settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((PublicTimelineSettingsFlyout)settingsFlyout).ViewModel.Type.Value =
                        (string)notification.Content;

                    ((PublicTimelineSettingsFlyout)settingsFlyout).ViewModel.ClearCommand.Execute();
                    ((PublicTimelineSettingsFlyout)settingsFlyout).ViewModel.UpdateCommand.Execute();

                    ((PublicTimelineSettingsFlyout)settingsFlyout).DataContext =
                        ((PublicTimelineSettingsFlyout)settingsFlyout).ViewModel;

                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "AddToCollection":
                    settingsFlyoutList =
                        _settingsFlyoutList.Where(x => x is AddStatusToCollectionSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new AddStatusToCollectionSettingsFlyout();
                        ((AddStatusToCollectionSettingsFlyout) settingsFlyout).ViewModel =
                            new AddStatusToCollectionSettingsFlyoutViewModel();
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    ((AddStatusToCollectionSettingsFlyout) settingsFlyout).ViewModel.IconSource.Value =
                        notification.UserIcon;
                    ((AddStatusToCollectionSettingsFlyout) settingsFlyout).ViewModel.Tokens.Value = notification.Tokens;

                    ((AddStatusToCollectionSettingsFlyout) settingsFlyout).ViewModel.ClearCommand.Execute();

                    ((AddStatusToCollectionSettingsFlyout) settingsFlyout).ViewModel.UserId.Value =
                        notification.Tokens.UserId;
                    ((AddStatusToCollectionSettingsFlyout) settingsFlyout).ViewModel.Status.Value =
                        (Status) notification.Content;

                    ((AddStatusToCollectionSettingsFlyout) settingsFlyout).ViewModel.UpdateCommand.Execute();

                    ((AddStatusToCollectionSettingsFlyout) settingsFlyout).DataContext =
                        ((AddStatusToCollectionSettingsFlyout) settingsFlyout).ViewModel;

                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "MainSetting":
                    settingsFlyoutList = _settingsFlyoutList.Where(x => x is MainSettingSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new MainSettingSettingsFlyout();
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "BehaviorSetting":
                    settingsFlyoutList =
                        _settingsFlyoutList.Where(x => x is BehaviorSettingSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new BehaviorSettingSettingsFlyout();
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "PostingSetting":
                    settingsFlyoutList = _settingsFlyoutList.Where(x => x is PostingSettingSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new PostingSettingSettingsFlyout();
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "DisplaySetting":
                    settingsFlyoutList = _settingsFlyoutList.Where(x => x is DisplaySettingSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new DisplaySettingSettingsFlyout();
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "NotificationSetting":
                    settingsFlyoutList =
                        _settingsFlyoutList.Where(x => x is NotificationSettingSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new NotificationSettingSettingsFlyout();
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "MuteSetting":
                    settingsFlyoutList = _settingsFlyoutList.Where(x => x is MuteSettingSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new MuteSettingSettingsFlyout();
                        ((MuteSettingSettingsFlyout) settingsFlyout).ViewModel =
                            new MuteSettingSettingsFlyoutViewModel();
                        settingsFlyout.DataContext = ((MuteSettingSettingsFlyout) settingsFlyout).ViewModel;
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    ((MuteSettingSettingsFlyout) settingsFlyout).ViewModel.MuteFilter.Value =
                        SettingService.Setting.MuteFilter;
                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "DatabaseSetting":
                    settingsFlyoutList =
                        _settingsFlyoutList.Where(x => x is DatabaseSettingSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new DatabaseSettingSettingsFlyout();
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "AccountsSetting":
                    settingsFlyoutList =
                        _settingsFlyoutList.Where(x => x is AccountsSettingSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new AccountsSettingSettingsFlyout();
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "AccountSetting":
                    settingsFlyoutList = _settingsFlyoutList.Where(x => x is AccountSettingSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new AccountSettingSettingsFlyout();
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    ((AccountSettingSettingsFlyout) settingsFlyout).ViewModel = notification.Content as AccountSetting;

                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "AdvancedSetting":
                    settingsFlyoutList =
                        _settingsFlyoutList.Where(x => x is AdvancedSettingSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new AdvancedSettingSettingsFlyout();
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "ColumnSetting":
                    settingsFlyoutList = _settingsFlyoutList.Where(x => x is ColumnSettingSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new ColumnSettingSettingsFlyout();
                        ((ColumnSettingSettingsFlyout) settingsFlyout).ViewModel =
                            new ColumnSettingSettingsFlyoutViewModel();
                        settingsFlyout.DataContext = ((ColumnSettingSettingsFlyout) settingsFlyout).ViewModel;
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    ((ColumnSettingSettingsFlyout) settingsFlyout).ViewModel.ColumnSetting.Value =
                        notification.Content as ColumnSetting;

                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "AppInfo":
                    settingsFlyoutList = _settingsFlyoutList.Where(x => x is AppInfoSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new AppInfoSettingsFlyout();
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
                case "AccountChange":
                    settingsFlyoutList = _settingsFlyoutList.Where(x => x is AccountChangeSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Any())
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new AccountChangeSettingsFlyout();
                        _settingsFlyoutList.Add(settingsFlyout);
                    }

                    settingsFlyout.Show();

                    PopupList.Insert(0, settingsFlyout);
                    break;
            }

            PopupList = PopupList.Where(x => x.IsOpen).Distinct().ToList();

            return null;
        }

        public static ShowSettingsFlyoutAction GetForCurrentView()
        {
            return _main;
        }

        public void HideTopPopup()
        {
            PopupList = PopupList.Where(x => x.IsOpen).Distinct().ToList();
            if (PopupList.Count == 0)
                return;

            var popup = PopupList.First();
            popup.Hide();
        }
    }

    public class ShowSettingsFlyoutNotification : Notification
    {
        /// <summary>
        ///     SettingsFlyoutの種類
        /// </summary>
        public string SettingsFlyoutType { get; set; }

        /// <summary>
        ///     WrapperのTokens
        /// </summary>
        public Tokens Tokens { get; set; }

        /// <summary>
        ///     ユーザーのアイコン
        /// </summary>
        public string UserIcon { get; set; }
    }
}