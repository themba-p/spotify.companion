using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace spotify.companion.View.Components
{
    public sealed partial class CollectionView : UserControl
    {
        public CollectionView()
        {
            this.InitializeComponent();

            WeakReferenceMessenger.Default.Register<Helpers.SelectionMessengerHelper>(this, (r, m) =>
            {
                HandleSelectionChangedMessenger(m);
            });
            WeakReferenceMessenger.Default.Register<Helpers.ViewMessengerHelper>(this, (r, m) =>
            {
                switch (m.Type)
                {
                    case Enums.ViewHelperType.ScrollToTop:
                        object payload = m.Payload ?? ContentView.Items.FirstOrDefault();
                        if (payload != null)
                            ContentView.ScrollIntoView(payload, ScrollIntoViewAlignment.Leading);
                        break;
                    case Enums.ViewHelperType.ClearSelected:
                        ContentView.SelectedItems.Clear();
                        break;
                    case Enums.ViewHelperType.SelectItems:
                        if (m.Payload != null && m.Payload is List<object> collection)
                        {
                            foreach (var item in collection)
                            {
                                ContentView.SelectedItems.Add(item);
                            }
                        }
                        break;
                    default:
                        break;
                }
            });
        }

        private void HandleSelectionChangedMessenger(Helpers.SelectionMessengerHelper e)
        {
            if (e.AddedItems != null)
            {
                foreach (var item in e.AddedItems)
                    ContentView.SelectedItems.Add(item);
            }

            if (e.RemovedItems != null)
            {
                foreach (var item in e.RemovedItems)
                    ContentView.SelectedItems.Remove(item);
            }
        }
    }
}
