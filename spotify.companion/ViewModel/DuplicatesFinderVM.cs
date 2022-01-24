using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using spotify.companion.Broker;
using spotify.companion.Enums;
using spotify.companion.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace spotify.companion.ViewModel
{
    internal partial class MainPageViewModel : ObservableObject
    {
        private void CloseDuplicatesFinder()
        {
            IsDuplicatePopupOpen = false;
            IsDuplicatePopupBusy = false;
            HasDuplicates = false;
            TotalDuplicates = 0;
            DuplicatesCollection.Clear();
            ClearSelected();
        }

        private async void FindDuplicates()
        {
            IsDuplicatePopupOpen = true;
            IsDuplicatePopupBusy = true;
            HasDuplicates = false;

            Action<TrackComparer> RemoveItemAction = new((item) =>
            {
                DuplicatesCollection.Remove(item);
                TotalDuplicates -= item.Count;
                if (TotalDuplicates < 0) TotalDuplicates = 0;
                HasDuplicates = TotalDuplicates > 0;

            });

            List<ItemBase> collection = SelectedItems.ToList();
            foreach (var item in collection)
            {
                if ((item as Playlist).Owner.Id == CurrentUser.Id)
                {
                    TrackComparer trackComparer = new((Playlist)item, RemoveItemAction);
                    DuplicatesCollection.Add(trackComparer);
                }
            }

            foreach (var trackComparer in DuplicatesCollection)
            {
                trackComparer.IsBusy = true;
                trackComparer.IsComplete = false;

                if (dispatcher != null)
                {
                    dispatcher.TryEnqueue(() =>
                    {
                        WeakReferenceMessenger.Default.Send(
                            new Helpers.ViewMessengerHelper(ViewHelperType.ScrollDuplicatesToView, trackComparer));
                    });
                }

                trackComparer.StatusText = "Processing...";
                trackComparer.Items = await DataBroker.GetTracksToCompareAsync(trackComparer.Playlist.Id);
                trackComparer.Compare();
                TotalDuplicates += trackComparer.Count;
                trackComparer.IsBusy = false;
                trackComparer.StatusText = "Complete";
                trackComparer.IsComplete = true;
            }

            List<TrackComparer> itemsToRemove = DuplicatesCollection.Where(c => c.Count == 0).ToList();
            foreach (var item in itemsToRemove)
                DuplicatesCollection.Remove(item);

            HasDuplicates = (DuplicatesCollection.Count > 0);
            IsDuplicatePopupBusy = false;
        }

        private async void RemoveDuplicates()
        {
            IsDuplicatePopupBusy = true;
            HasDuplicates = false;

            var collection = DuplicatesCollection.Where(c => c.Count > 0).ToList();
            if (collection == null) return;
            foreach (var item in collection)
            {
                await DataBroker.DeleteTracksAsync(item.Playlist.Id, item.UrisToRemove);
                item.Count = 0;
            }

            if (dispatcher != null)
            {
                dispatcher.TryEnqueue(() =>
                {
                    InAppNotification notification = new(ResponseType.Success, "Duplicates removed successfully", "", true);
                    WeakReferenceMessenger.Default.Send(notification);
                });
            }

            IsDuplicatePopupBusy = false;
            CloseDuplicatesFinder();
        }

        private ObservableCollection<TrackComparer> _duplicatesCollection = new();
        public ObservableCollection<TrackComparer> DuplicatesCollection
        {
            get => _duplicatesCollection;
            set => SetProperty(ref _duplicatesCollection, value);
        }

        public RelayCommand FindDuplicatesCommand { get; }
        public RelayCommand CloseDuplicateFinderCommand { get; }
        public RelayCommand RemoveDuplicatesCommand { get; }

        private bool _isDuplicatePopupOpen;
        public bool IsDuplicatePopupOpen
        {
            get => _isDuplicatePopupOpen;
            private set
            {
                IsPopupDismissPanelOpen = value;
                SetProperty(ref _isDuplicatePopupOpen, value);
            }
        }

        private bool _isDuplicatePopupBusy;
        public bool IsDuplicatePopupBusy
        {
            get => _isDuplicatePopupBusy;
            private set => SetProperty(ref _isDuplicatePopupBusy, value);
        }

        private bool _hasDuplicates;
        public bool HasDuplicates
        {
            get => _hasDuplicates;
            private set => SetProperty(ref _hasDuplicates, value);
        }

        private int _totalDuplicates = 0;
        public int TotalDuplicates
        {
            get => _totalDuplicates;
            private set => SetProperty(ref _totalDuplicates, value);
        }
    }
}
