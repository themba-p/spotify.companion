using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using spotify.companion.Broker;
using spotify.companion.Enums;
using spotify.companion.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace spotify.companion.ViewModel
{
    internal partial class MainPageViewModel : ObservableObject
    {
        private void ShowPopupDialog(PopupDialogType popupDialogType)
        {
            CurrentPopupType = popupDialogType;

            Action primaryAction = popupDialogType switch
            {
                PopupDialogType.ClearLiked => ClearLiked,
                PopupDialogType.MergePlaylist => MergeSelected,
                PopupDialogType.UnfollowPlaylist => UnfollowSelected,
                _ => null,
            };

            PopupObj.Show(popupDialogType, SelectedItems.ToList(), primaryAction);

            IsPopupOpen = true;
        }

        private void ClosePopupDialog()
        {
            IsPopupOpen = false;
            PopupObj.Reset();
            IsPopupBusy = false;
            CurrentPopupType = PopupDialogType.None;
        }

        private async void MergeSelected()
        {
            IsPopupBusy = true;

            List<ItemBase> collection = new(SelectedItems);
            List<string> uris = new();
            foreach (var item in collection)
            {
                var playlistUris = await DataBroker.GetTracksUrisAsync(item.Id);
                if (playlistUris != null)
                {
                    foreach (var uri in playlistUris)
                    {
                        if (!uris.Contains(uri))
                            uris.Add(uri);
                    }
                }
            }

            Action<Playlist> callback = new((playlist) =>
            {
                AddItem(playlist);

                if (dispatcher != null)
                {
                    dispatcher.TryEnqueue(() =>
                    {
                        string message = (playlist != null) ? "Successfully Merged." : "Error merging playlists.";
                        ResponseType type = (playlist != null) ? ResponseType.Success : ResponseType.Error;

                        InAppNotification notification = new(type, message, "", true);
                        WeakReferenceMessenger.Default.Send(notification);

                        if (!PopupObj.IsCheckBoxChecked)
                        {
                            ClosePopupDialog();
                            ClearSelected();
                        }
                        else if (playlist != null)
                        {
                            UnfollowSelected();
                        }

                        IsPopupBusy = false;

                        RefreshFilters();

                        WeakReferenceMessenger.Default.Send(
                            new Helpers.ViewMessengerHelper(ViewHelperType.ScrollToTop, playlist));
                    });
                }

            });

            await DataBroker.MergePlaylists(PopupObj.NameInputText, CurrentUser.Id, uris, callback);
        }

        private async void CloneSelected()
        {
            IsPlaylistsLoading = true;

            int success = 0;
            int proccessed = 0;
            int total = SelectedItems.Count;

            List<ItemBase> collection = new(SelectedItems);
            Action<Playlist> callback = null;

            Action completedCallback = new(() =>
            {
                if (dispatcher != null)
                {
                    dispatcher.TryEnqueue(() =>
                    {
                        ClearSelected();

                        IsPlaylistsLoading = false;

                        string message = "Successfully Copied " + success + "/" + collection.Count;

                        WeakReferenceMessenger.Default.Send(
                        new Helpers.ViewMessengerHelper(ViewHelperType.ScrollToTop, ItemsCollection.LastOrDefault()));

                        InAppNotification notification = new(ResponseType.Success, message, "", true);
                        WeakReferenceMessenger.Default.Send(notification);
                    });
                }
                if (success > 0) RefreshFilters();
            });

            foreach (var item in collection)
            {
                callback = new((playlist) =>
                {
                    proccessed += 1;
                    if (playlist != null)
                    {
                        AddItem(playlist);
                        success += 1;
                    }

                    if (proccessed >= total) completedCallback();
                });
                await DataBroker.ClonePlaylist((Playlist)item, CurrentUser?.Id, callback);
            }
        }

        private async void UnfollowSelected()
        {
            IsPopupBusy = true;

            int success = 0;

            List<ItemBase> collection = new(SelectedItems);
            foreach (var item in collection)
            {
                if (await DataBroker.UnfollowPlaylistAsync(item.Id))
                {
                    RemoveItem(item);
                    success += 1;
                }
            }

            ClearSelected();

            if (dispatcher != null)
            {
                dispatcher.TryEnqueue(() =>
                {
                    string message = "Successfully Unfollowed " + success + " of " + collection.Count;

                    InAppNotification notification = new(ResponseType.Success, message, "", true);
                    WeakReferenceMessenger.Default.Send(notification);
                });
            }

            IsPopupBusy = false;
            ClosePopupDialog();
        }

        private async void PlaySelected()
        {
            IsPlaylistsLoading = true;

            List<string> uris = await GetPlaylistTracksUris(SelectedItems);
            if (uris != null) await DataBroker.PlayItems(uris);
            ClearSelected();

            IsPlaylistsLoading = false;
        }

        private async Task SaveLikedToPlaylist(IEnumerable<string> uris = null, Action<bool> callbackFunc = null)
        {
            IsPlaylistsLoading = true;

            if (uris == null || !uris.Any())
            {
                var liked = await DataBroker.GetLikedAsync();
                if (liked != null)
                {
                    uris = liked.Select(c => c.Uri);
                }
            }

            if (uris == null || !uris.Any())
            {
                IsPlaylistsLoading = false;
                return;
            }

            string name = "";
            if (ItemsCollection.Where(c => c.DisplayName == "Liked songs").FirstOrDefault() == null)
                name = "Liked songs";
            else
            {
                for (int i = 1; i < ItemsCollection.Count; i++)
                {
                    name = "Liked songs(" + i + ")";
                    if (ItemsCollection.Where(c => c.DisplayName == name).FirstOrDefault() == null)
                        break;

                }
            }

            Action<Task<SpotifyAPI.Web.FullPlaylist>> callback = new(async (result) =>
            {
                if (result != null && result.Result != null)
                {
                    var playlist = result.Result;
                    var success = await DataBroker.AddToPlaylist(playlist.Id.ToString(), uris);

                    string message = (success) ? "Liked songs saved successfully" : "Error saving Liked songs";
                    ResponseType type = (success) ? ResponseType.Success : ResponseType.Error;

                    if (dispatcher != null)
                    {
                        dispatcher.TryEnqueue(async () =>
                        {
                            IsPlaylistsLoading = false;

                            var item = await DataBroker.GetPlaylistAsync(playlist.Id);
                            AddItem(item);

                            InAppNotification notification = new(type, message, "", true);
                            WeakReferenceMessenger.Default.Send(notification);

                            RefreshFilters();

                            WeakReferenceMessenger.Default.Send(
                                new Helpers.ViewMessengerHelper(ViewHelperType.ScrollToTop, item));
                        });
                    }

                    callbackFunc?.Invoke(success);
                }
                else callbackFunc?.Invoke(false);
            });

            await DataBroker.CreatePlaylistAsync(CurrentUser.Id, name, CurrentUser.DisplayName + "'s Liked songs.", callback);
        }
        
        private async void ClearLiked()
        {
            IsPopupBusy = true;

            IEnumerable<string> uris = null;

            Action<bool> callback = new(async (success) =>
            {
                if (!success)
                {
                    if (dispatcher != null)
                    {
                        dispatcher.TryEnqueue(() =>
                        {
                            IsPopupBusy = false;
                            InAppNotification notification = new(ResponseType.Error, "Error saving Liked songs. Aborting", "", true);
                            WeakReferenceMessenger.Default.Send(notification);
                        });
                    }
                    return;
                }

                string message;
                ResponseType type = ResponseType.Success;
                success = await DataBroker.ClearLikedAsync(uris);

                if (!success)
                {
                    message = "Error occured, try again.";
                    type = ResponseType.Error;
                }
                else
                    message = "Liked songs cleared";

                if (dispatcher != null)
                {
                    dispatcher.TryEnqueue(() =>
                    {
                        IsPopupBusy = false;
                        InAppNotification notification = new(type, message, "", true);
                        WeakReferenceMessenger.Default.Send(notification);
                    });
                }

                ClosePopupDialog();
            });

            if (PopupObj.IsCheckBoxChecked)
            {
                var liked = await DataBroker.GetLikedAsync();
                if (liked != null)
                {
                    uris = liked.Select(c => c.Uri);
                    await SaveLikedToPlaylist(uris, callback);
                }
            }
            else
                callback(true);
        }

        public RelayCommand PlaySelectedCommand { get; }
        public RelayCommand CloneSelectedCommand { get; }
        public RelayCommand UnfollowSelectedCommand { get; }
        public RelayCommand MergeSelectedCommand { get; }
        public RelayCommand ClosePlaylistPopupCommand { get; }
        public RelayCommand CancelPopupCommand { get; }
        public RelayCommand ClearLikedCommand { get; }
        public AsyncRelayCommand SaveLikedCommand { get; }

        #region Fields

        private PopupObj _popupObj = new();
        public PopupObj PopupObj
        {
            get => _popupObj;
            private set => SetProperty(ref _popupObj, value);
        }

        private bool _isPopupDismissPanelOpen;
        public bool IsPopupDismissPanelOpen
        {
            get => _isPopupDismissPanelOpen;
            private set => SetProperty(ref _isPopupDismissPanelOpen, value);
        }

        private bool _isPopupOpen;
        public bool IsPopupOpen
        {
            get => _isPopupOpen;
            private set
            {
                IsPopupDismissPanelOpen = value;
                SetProperty(ref _isPopupOpen, value);
            }
        }

        private bool _isPopupBusy;
        public bool IsPopupBusy
        {
            get => _isPopupBusy;
            private set => SetProperty(ref _isPopupBusy, value);
        }

        private PopupDialogType _currentPopupType;
        public PopupDialogType CurrentPopupType
        {
            get => _currentPopupType;
            private set => SetProperty(ref _currentPopupType, value);
        }
        
        #endregion
    }
}
