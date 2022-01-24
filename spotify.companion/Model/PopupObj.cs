using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using spotify.companion.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spotify.companion.Model
{
    internal class PopupObj : ObservableObject
    {
        public PopupObj()
        {
            this.Reset();
        }

        private RelayCommand _primaryCommand;
        public RelayCommand PrimaryCommand
        {
            get => _primaryCommand;
            set => SetProperty(ref _primaryCommand, value);
        }

        public void Show(PopupDialogType popupDialogType, List<ItemBase> collection, Action primaryAction)
        {
            PrimaryCommand = new(primaryAction);

            Title = popupDialogType switch
            {
                PopupDialogType.ClearLiked => "Remove Liked songs",
                PopupDialogType.MergePlaylist => "Merge playlists",
                PopupDialogType.UnfollowPlaylist => string.Concat("Delete ", (collection.Count == 1) ? "playlist" : "playlists"),
                _ => "Popup",
            };

            string conStr; ;
            string action = (popupDialogType == PopupDialogType.MergePlaylist) ? "Merge " : "Delete ";

            if (collection.Count > 2)
            {
                conStr = string.Concat(action,
                    collection[0].DisplayName, ", ",
                    collection[1].DisplayName,
                    " and ", collection.Count - 2, " others?");
            }
            else if (collection.Count == 2)
            {
                conStr = string.Concat(action,
                    collection[0].DisplayName, " and ",
                    collection[1].DisplayName, "?");
            }
            else
            {
                conStr = string.Concat(action,
                    collection.FirstOrDefault()?.DisplayName, "?");
            }

            SubTitle = popupDialogType switch
            {
                PopupDialogType.ClearLiked => "Are you sure you want to remove all Liked songs, this cannot be undone!",
                PopupDialogType.MergePlaylist => conStr,
                PopupDialogType.UnfollowPlaylist => conStr,
                _ => "Popup",
            };

            PrimaryButtonText = popupDialogType switch
            {
                PopupDialogType.ClearLiked => "Clear Liked",
                PopupDialogType.MergePlaylist => "Merge",
                PopupDialogType.UnfollowPlaylist => "Delete",
                _ => "Confirm",
            };

            ShowNameInput = popupDialogType switch
            {
                PopupDialogType.UnfollowPlaylist => false,
                PopupDialogType.ClearLiked => false,
                _ => true,
            };

            ShowCheckBox = popupDialogType switch
            {
                PopupDialogType.ClearLiked => true,
                PopupDialogType.MergePlaylist => true,
                _ => false,
            };

            CheckBoxLabel = popupDialogType switch
            {
                PopupDialogType.ClearLiked => "Backup to playlist before clearing?",
                PopupDialogType.MergePlaylist => "Unfollow playlists after merging?",
                _ => "",
            };
        }

        public void Reset()
        {
            PrimaryCommand = null;
            Title = "";
            SubTitle = "";
            NameInputText = "";
            ShowNameInput = false;
            CheckBoxLabel = "";
            IsCheckBoxChecked = false;
            ShowCheckBox = false;
            PrimaryButtonText = "Confirm";
        }

        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _subTitle;
        public string SubTitle
        {
            get => _subTitle;
            set => SetProperty(ref _subTitle, value);
        }

        private string _nameInputText;
        public string NameInputText
        {
            get => _nameInputText;
            private set => SetProperty(ref _nameInputText, value);
        }

        private bool _showNameInput = true;
        public bool ShowNameInput
        {
            get => _showNameInput;
            private set => SetProperty(ref _showNameInput, value);
        }

        private string _checkBoxLabel;
        public string CheckBoxLabel
        {
            get => _checkBoxLabel;
            private set => SetProperty(ref _checkBoxLabel, value);
        }

        private bool _isCheckBoxChecked;
        public bool IsCheckBoxChecked
        {
            get => _isCheckBoxChecked;
            private set => SetProperty(ref _isCheckBoxChecked, value);
        }

        private bool _showCheckBox;
        public bool ShowCheckBox
        {
            get => _showCheckBox;
            private set => SetProperty(ref _showCheckBox, value);
        }

        private string _primaryButtonText = "Confirm";
        public string PrimaryButtonText
        {
            get => _primaryButtonText;
            private set => SetProperty(ref _primaryButtonText, value);
        }
    }
}
