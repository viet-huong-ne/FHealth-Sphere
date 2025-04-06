using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.HealthRecordModelViews
{
    public class WeeklyMetricViewModel
    {
        public string WeekStartDate { get; set; }
        public int? PatientId { get; set; }
        public decimal? WeeklyAverage { get; set; } 
        public List<DailyHealthRecordModel> DailyAverages { get; set; } = new List<DailyHealthRecordModel>();
    }
}
