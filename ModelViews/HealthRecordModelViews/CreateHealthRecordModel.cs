using ModelViews.RecordMetricItemModelViews;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.HealthRecordModelViews
{
    public class CreateHealthRecordModel
    {
        public int? PatientId { get; set; }
        public string? BandId { get; set; }
        public DateTime? Date {get; set; }
        public string GhiChu { get; set; }
        public List<CreateRecordMetricItemModel> RecordMetricItems { get; set; }
    }
}
