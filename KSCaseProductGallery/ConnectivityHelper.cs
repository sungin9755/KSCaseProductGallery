using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Networking;

namespace KSCaseProductGallery
{
    public static class ConnectivityHelper
    {
        public static bool IsInternetAvailable()
        {
            var access = Connectivity.Current.NetworkAccess;
            return access == NetworkAccess.Internet;
        }
    }
}
