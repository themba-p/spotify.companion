using System.Collections.Generic;

namespace spotify.companion.Helpers
{
    internal class SelectionMessengerHelper
    {
        public IList<object> AddedItems { get; set; }
        public IList<object> RemovedItems { get; set; }

        public SelectionMessengerHelper(IList<object> addedItems, IList<object> removedItems)
        {
            this.AddedItems = addedItems;
            this.RemovedItems = removedItems;   
        }
    }
}
