using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spotify.companion.Model
{
    internal class TrackComparer : ObservableObject
    {
        public TrackComparer(Playlist playlist, Action<TrackComparer> removeItemFunc)
        {
            RemoveItemCommand = new(() => removeItemFunc(this));
            this.Playlist = playlist;
        }

        public void Compare()
        {
            if (this.Items == null || !this.Items.Any()) return;

            List<string> temp = new();
            int total = this.Items.Count;
            int index = 1;
            this.Items.ForEach(item =>
            {
                this.StatusText = "processing " + index + " of " + total;
                if (temp.Where(c => c == item.ISRCCode).FirstOrDefault() != null)
                {
                    UrisToRemove.Add(item.Uri);
                    Count += 1;
                } else
                {
                    temp.Add(item.ISRCCode);
                }

                index++;
            });
        }

        public List<TrackCompareItem> Items { get; set; }
        public List<string> UrisToRemove = new();

        private Playlist _playlist;
        public Playlist Playlist
        {
            get => _playlist;
            set => SetProperty(ref _playlist, value);
        }

        private int _count = 0;
        public int Count
        {
            get => _count;
            set => SetProperty(ref _count, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private bool _isComplete;
        public bool IsComplete
        {
            get => _isComplete;
            internal set => SetProperty(ref _isComplete, value);
        }

        private string _statusText = "Loading";
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public RelayCommand RemoveItemCommand { get; }
    }
}
