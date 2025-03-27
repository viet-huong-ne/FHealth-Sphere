using ModelViews.RecordMetricItemModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.HealthRecordModelViews
{
    public class DailyHealthRecordModel
    {
        public string Date { get; set; }
        public int? PatientId { get; set; }
        public List<RecordMetricItemModel> Metrics { get; set; } = new List<RecordMetricItemModel>();
    }
}
