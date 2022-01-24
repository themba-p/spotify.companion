using spotify.companion.Enums;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spotify.companion.Model
{
    internal class TrackCompareItem : ItemBase
    {
        public TrackCompareItem() { }

        public TrackCompareItem(string id, Enums.ItemType itemType, string displayName,
            string uri, string imageSource, bool isSelected)
            : base(id, itemType, displayName, uri, imageSource, isSelected)
        {

        }


        public string ISRCCode { get; set; }

        public static TrackCompareItem Convert(FullTrack item)
        {
            string code = "";
            if (item.ExternalIds.Count > 0)
                code = item.ExternalIds.FirstOrDefault().Value;

            if (string.IsNullOrEmpty(code)) return null;

            return new TrackCompareItem
            {
                Id = item.Id,
                ISRCCode = code,
                Uri = item.Uri,
            };
        }
    }
}
