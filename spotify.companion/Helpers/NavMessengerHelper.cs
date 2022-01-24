using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spotify.companion.Helpers
{
    internal class NavMessengerHelper
    {
        public NavMessengerHelper(Enums.NavTargetType navTargetType)
        {
            this.NavTargetType = navTargetType;
        }

        public Enums.NavTargetType NavTargetType { get; set; }
    }
}
