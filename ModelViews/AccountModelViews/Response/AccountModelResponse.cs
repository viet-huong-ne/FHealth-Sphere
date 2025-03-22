using ModelViews.PatientInfoModelViews.Response;
using ModelViews.WatcherModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.AccountModelViews.Response
{
    public class AccountModelResponse
    {
        public int Id { get; set; }
        public string Role { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FCMToken { get; set; }
        public PatientInfoResponse? PatientInfo { get; set; }
        public List<WatcherResponse>? WatcherResponses { get; set; }

    }
}
