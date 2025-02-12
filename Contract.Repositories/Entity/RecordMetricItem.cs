using Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Repositories.Entity
{
    public class RecordMetricItem : BaseEntity
    {
        [Key]
        public int ID { get; set; }
        public int RecordID { get; set; }
        public int MetricID { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }

        public virtual HealthRecord HealthRecord { get; set; }
        public virtual Metric Metric { get; set; }
    }
}
