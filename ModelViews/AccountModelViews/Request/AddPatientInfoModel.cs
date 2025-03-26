using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.AccountModelViews.Request
{
    public class AddPatientInfoModel
    {
        public string? Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? FCMToken { get; set; }
    }
}
