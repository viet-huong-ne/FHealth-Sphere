using ModelViews.RecordMetricItemModelViews;
using System.Collections.Generic;

namespace ModelViews.HealthRecordModelViews
{
    public class CreateHealthRecordCombinedModel
    {
        public int? PatientId { get; set; }
        public int? BandId { get; set; }
        public string GhiChu { get; set; }
        public List<CreateRecordMetricItemModel> RecordMetricItems { get; set; }
    }

    public class UpdateHealthRecordCombinedModel
    {
        public int? PatientId { get; set; }
        public int? BandId { get; set; }
        public string GhiChu { get; set; }
        public List<UpdateRecordMetricItemModel> RecordMetricItems { get; set; }
    }

    public class HealthRecordCombinedViewModel
    {
        public int Id { get; set; }
        public int? PatientId { get; set; }
        public int? BandId { get; set; }
        public string GhiChu { get; set; }
        public List<RecordMetricItemViewModel> RecordMetricItems { get; set; }
    }

    public class RecordMetricItemViewModel
    {
        public int Id { get; set; }
        public int? MetricId { get; set; }
        public decimal? Value { get; set; }
        public string Type { get; set; }
    }
}