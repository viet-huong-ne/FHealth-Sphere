using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Service
{
    public class FirebasePollingService
    {
        private readonly FirebaseClient _firebaseClient;
        private readonly Timer _timer;

        public FirebasePollingService()
        {
            _firebaseClient = new FirebaseClient("https://your-database-url.firebaseio.com/");
            _timer = new Timer(async _ => await CheckForUpdates(), null, TimeSpan.Zero, TimeSpan.FromSeconds(5)); // Quét mỗi 5 giây
        }

        private async Task CheckForUpdates()
        {
            try
            {
                var data = await _firebaseClient.Child("your-data-key").OnceAsync<object>();
                Console.WriteLine($"Fetched {data.Count} items from Firebase at {DateTime.Now}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching data: {ex.Message}");
            }
        }
    }
}
