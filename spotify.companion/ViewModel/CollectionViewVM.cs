using CommunityToolkit.WinUI.UI;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
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
        private async void LoadData()
        {
            IsPlaylistsLoading = true;

            ItemsCollection.Clear();

            List<Playlist> collection = await DataBroker.GetPlaylistsAsync();

            if (collection == null || collection.Count == 0)
            {
                IsPlaylistsLoading = false;
                return;
            }

            collection.ForEach(item => AddItem(item));

            IsPlaylistsLoading = false;
        }

        private void AddItem(ItemBase item)
        {
            if (item == null) return;

            if (dispatcher != null)
            {
                dispatcher.TryEnqueue(() =>
                {
                    if (ItemsCollection.Where(c => c.Id == item.Id).FirstOrDefault() == null)
                    {
                        item.SelectedChanged = ItemBaseIsSelectedChanged;
                        ItemsCollection.Add(item);
                    }
                });
            }
        }

        private void RemoveItem(ItemBase item)
        {
            if (item == null) return;

            item.SelectedChanged = null;
            ItemsCollection.Remove(item);
            if (item.IsSelected) RemoveSelected(new List<ItemBase> { item });
        }

        private void ItemBaseIsSelectedChanged(ItemBase itemBase)
        {
            if (itemBase == null) return;

            if (!itemBase.IsSelected)
                RemoveSelected(new List<ItemBase> { itemBase });
            else
                AddSelected(new List<ItemBase> { itemBase });
        }

        private void AddSelected(List<ItemBase> collection)
        {
            collection.ForEach((item) =>
            {
                if (!SelectedItems.Contains(item))
                    SelectedItems.Add(item);
            });
        }

        private void RemoveSelected(List<ItemBase> collection)
        {
            collection.ForEach((item) => SelectedItems.Remove(item));
        }

        private void ClearSelected()
        {
            var selected = SelectedItems.ToList();
            selected.ForEach(c => c.IsSelected = false);
            SelectedItems.Clear();
        }

        private void SelectedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SelectedPreviewUrl = SelectedItems.LastOrDefault()?.ImageSource;
            NoSelectedItems = (SelectedItems.Count == 0);
            CanMerge = (SelectedItems.Count > 1);
        }

        private static void FilterCollection(AdvancedCollectionView collectionView, CategoryType categoryType, string searchText)
        {
            if (collectionView == null) return;

            if (!string.IsNullOrEmpty(searchText))
            {
                if (categoryType == CategoryType.Playlists)
                {
                    collectionView.Filter = c => (((Playlist)c).DisplayName).Contains(searchText, StringComparison.CurrentCultureIgnoreCase) ||
                    (((Playlist)c).Description).Contains(searchText, StringComparison.CurrentCultureIgnoreCase) ||
                    (((Playlist)c).Owner.DisplayName).Contains(searchText, StringComparison.CurrentCultureIgnoreCase);
                }
            }
            else
            {
                collectionView.Filter = c => c != null;
            }

            WeakReferenceMessenger.Default.Send(new Helpers.ViewMessengerHelper(ViewHelperType.ScrollToTop));
        }

        private void SortCollection(Sorting sorting)
        {
            using (AdvancedCollectionView.DeferRefresh())
            {
                if (sorting == null) return;
                AdvancedCollectionView.SortDescriptions.Clear();
                if (sorting.Type != SortingType.Default)
                {
                    AdvancedCollectionView.SortDescriptions.Add(new SortDescription(sorting.Property, SortDirection.Ascending));
                }
            }

            WeakReferenceMessenger.Default.Send(new Helpers.ViewMessengerHelper(ViewHelperType.ScrollToTop));
        }

        private void RefreshFilters()
        {
            dispatcher?.TryEnqueue(() =>
            {
                if (!string.IsNullOrEmpty(FilterText))
                    FilterCollection(AdvancedCollectionView, SelectedCategory.Type, FilterText);
                else
                    SortCollection(SelectedSortType);
            });
        }


        private AdvancedCollectionView _advancedCollectionView;
        public AdvancedCollectionView AdvancedCollectionView
        {
            get => _advancedCollectionView;
            set => SetProperty(ref _advancedCollectionView, value);
        }

        private ObservableCollection<ItemBase> _itemsCollection = new();
        public ObservableCollection<ItemBase> ItemsCollection
        {
            get => _itemsCollection;
            set => SetProperty(ref _itemsCollection, value);
        }

        private ObservableCollection<ItemBase> _selectedItems = new();
        public ObservableCollection<ItemBase> SelectedItems
        {
            get => _selectedItems;
            set => SetProperty(ref _selectedItems, value);
        }

        public RelayCommand ClearSelectedCommand { get; }
        public RelayCommand SelectAllCommand { get; }
        public RelayCommand RefreshCommand { get; }
    }
}
