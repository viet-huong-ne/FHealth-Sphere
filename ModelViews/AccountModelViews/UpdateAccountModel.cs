using ModelViews.AccountModelViews.Request;
using ModelViews.PatientInfoModelViews.Response;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.AccountModelViews
{
    public class UpdateAccountModel
    {
        public string? Role { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FCMToken { get; set; }
        public UpdatePaitientInfo? PatientInfo { get; set; }
    }
}
