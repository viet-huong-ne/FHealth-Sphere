using Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Contract.Repositories.Entity
{
    public class RecordMetricItem : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public int? MetricId { get; set; }
        public int? HealthRecordId { get; set; }
        public decimal? Value { get; set; }
        public string? Type { get; set; }
        [JsonIgnore]
        public virtual HealthRecord? HealthRecord { get; set; }
        [JsonIgnore]
        public virtual Metric? Metric { get; set; }
    }
}
