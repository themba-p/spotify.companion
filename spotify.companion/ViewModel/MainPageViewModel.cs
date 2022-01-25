using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Linq;
using Microsoft.Toolkit.Mvvm.Messaging;
using spotify.companion.Model;
using System.Collections.ObjectModel;
using spotify.companion.Enums;
using System.Collections.Generic;
using spotify.companion.Broker;
using Microsoft.Toolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI;
using System.Threading.Tasks;
using spotify.companion.Services;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace spotify.companion.ViewModel
{
    internal partial class MainPageViewModel : ObservableObject
    {
        private readonly DispatcherQueue dispatcher = DispatcherQueue.GetForCurrentThread();

        public MainPageViewModel()
        {
            PopupDismissPanelCommand = new RelayCommand(() =>
            {
                IsPopupOpen = false;
                IsDuplicatePopupOpen = false;
                IsPopupDismissPanelOpen = false;
            });
            LogoutCommand = new RelayCommand(() =>
            {
                AdvancedCollectionView.Clear();
                SelectedItems.Clear();
                CurrentUser = null;

                AuthBroker.Logout();
                Ioc.Default.GetService<INavigationService>().Navigate<LoginPageViewModel>();
            });
            ClosePlaylistPopupCommand = new(ClosePopupDialog);
            CancelPopupCommand = new(ClosePopupDialog);

            /*** see ViewModel/DuplicatesFinderVM.cs ***/

            FindDuplicatesCommand = new(FindDuplicates);
            RemoveDuplicatesCommand = new(RemoveDuplicates);
            CloseDuplicateFinderCommand = new(CloseDuplicatesFinder);

            /** -----------------------------------**/

            /*** see ViewModel/PlaylistPopupVMd.cs ***/

            SaveLikedCommand = new(async () => await SaveLikedToPlaylist());
            ClearLikedCommand = new RelayCommand(() =>
            {
                ShowPopupDialog(PopupDialogType.ClearLiked);
            });
            MergeSelectedCommand = new RelayCommand(() => ShowPopupDialog(PopupDialogType.MergePlaylist));
            UnfollowSelectedCommand = new RelayCommand(() => ShowPopupDialog(PopupDialogType.UnfollowPlaylist));
            CloneSelectedCommand = new(CloneSelected);
            PlaySelectedCommand = new(PlaySelected);

            /** -----------------------------------**/

            /*** see ViewModel/CollectionViewVM.cs ***/

            RefreshCommand = new RelayCommand(() =>
            {
                SelectedItems.Clear();
                ItemsCollection.Clear();
                SelectedSortType = SortingList.FirstOrDefault();
                FilterText = "";

                LoadData();
            });
            SelectAllCommand = new RelayCommand(() =>
            {
                if (dispatcher != null)
                {
                    dispatcher.TryEnqueue(() =>
                    {
                        IsPlaylistsLoading = true;

                        var items = ItemsCollection.ToList();
                        items.ForEach(item => item.IsSelected = true);

                        IsPlaylistsLoading = false;
                    });
                }
            });
            ClearSelectedCommand = new(ClearSelected);
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;

            /** -----------------------------------**/

            WeakReferenceMessenger.Default.Register<User>(this, (r, m) => CurrentUser = m);
            Initialize();
        }

        private void Initialize()
        {
            SelectedCategory = Categories.First();
            AdvancedCollectionView = new AdvancedCollectionView(ItemsCollection, true);

            LoadData();
        }

        private static async Task<List<string>> GetPlaylistTracksUris(IEnumerable<ItemBase> playlists)
        {
            List<string> urisResults = new();
            foreach (var playlist in playlists)
            {
                List<string> uris = await DataBroker.GetTracksUrisAsync(playlist.Id);
                if (uris != null) urisResults.AddRange(uris);
            }
            return urisResults;
        }

        public RelayCommand LogoutCommand { get; }
        public RelayCommand PopupDismissPanelCommand { get; }

        private ObservableCollection<Category> _categories;
        public ObservableCollection<Category> Categories {
            get
            {
                if (_categories != null) return _categories;

                return _categories = new ObservableCollection<Category>
                {
                    //new Category("Liked", CategoryType.Liked),
                    //new Category("Albums", CategoryType.Albums),
                    new Category("Playlists", CategoryType.Playlists),
                };
            }
        }

        private ObservableCollection<Sorting> _sortingList;
        public ObservableCollection<Sorting> SortingList
        {
            get
            {
                if (_sortingList != null) return _sortingList;

                return _sortingList = new ObservableCollection<Sorting>
                {
                    new Sorting("Recently added", "", SortingType.Default),
                    new Sorting("Alphabetical", "DisplayName", SortingType.Name),
                    new Sorting("Creator", "Creator", SortingType.Owner),
                };
            }
        }

        private string _filterText;
        public string FilterText
        {
            get => _filterText;
            set
            {
                SetProperty(ref _filterText, value);
                FilterCollection(AdvancedCollectionView, SelectedCategory.Type, value);
            }
        }

        private Sorting _selectedSortType;
        public Sorting SelectedSortType
        {
            get => _selectedSortType;
            set
            {
                SetProperty(ref _selectedSortType, value);
                SortCollection(value);
            }
        }

        private Category _selectedCategory;
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                SetProperty(ref _selectedCategory, value);
                if (value != null) FilterCollection(AdvancedCollectionView, value.Type, FilterText);
            }
        }

        private string _selectedPreviewUrl;
        public string SelectedPreviewUrl
        {
            get => _selectedPreviewUrl;
            set => SetProperty(ref _selectedPreviewUrl, value);
        }

        private User _currentUser;
        public User CurrentUser
        {
            get => _currentUser;
            private set => SetProperty(ref _currentUser, value);
        }

        private bool _noSelectedItems = true;
        public bool NoSelectedItems
        {
            get => _noSelectedItems;
            private set => SetProperty(ref _noSelectedItems, value);
        }

        private bool _canMerge;
        public bool CanMerge
        {
            get => _canMerge;
            private set => SetProperty(ref _canMerge, value);
        }

        private bool _isPlaylistsLoading;
        public bool IsPlaylistsLoading
        {
            get => _isPlaylistsLoading;
            private set => SetProperty(ref _isPlaylistsLoading, value);
        }
    }
}
