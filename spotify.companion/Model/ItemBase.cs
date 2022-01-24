using System;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using spotify.companion.Enums;

namespace spotify.companion.Model
{
    public class ItemBase : ObservableObject
    {
        public ItemBase() { }

        public ItemBase(string id, ItemType itemType, string displayName, 
            string uri, string imageSource, bool isSelected) {
            this.Id = id;
            this.ItemType = itemType;
            this.DisplayName = displayName;
            this.Uri = uri;
            this.ImageSource = imageSource;
            this.IsSelected = isSelected;
        }

        public Action<ItemBase> SelectedChanged { get; set; }

        private string _id;
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private ItemType _itemType;
        public ItemType ItemType
        {
            get => _itemType;
            set => SetProperty(ref _itemType, value);
        }

        private string _displayName;
        public string DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        private string _uri;
        public string Uri
        {
            get => _uri;
            set => SetProperty(ref _uri, value);
        }

        private string _imageSource;
        public string ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                SetProperty(ref _isSelected, value);
                if (SelectedChanged != null) SelectedChanged.Invoke(this);
            }
        }
    }
}
