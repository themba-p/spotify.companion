using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace spotify.companion.Helpers
{
    internal class Helpers
    {
        public async static Task<bool> IsConnectedToInternet()
        {
            try
            {
                string uri = "http://google.com/generate_204";
                using var client = new HttpClient();
                using (await client.GetAsync(uri))
                    return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
