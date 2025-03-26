using Contract.Repositories.Entity;
using Firebase.Database;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using ModelViews.HealthRecordModelViews;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Service
{
    public class FirebaseService
    {
        private readonly FirebaseClient _firebaseClient;

        public FirebaseService()
        {

            _firebaseClient = new FirebaseClient("https://fhealth-sphere---login-default-rtdb.asia-southeast1.firebasedatabase.app/");
        }

        // Hàm gửi dữ liệu lên Firebase
        public async Task<bool> AddHealthRecordAsync(CreateHealthRecordCombinedModel record)
        {
            string jsonData = JsonConvert.SerializeObject(record);
            var result = await _firebaseClient
                .Child("healthRecordsV2")
                .PostAsync(jsonData);

            return result != null;
        }
    }
}
