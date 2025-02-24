using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.RecordMetricItemModelViews
{
    public class CreateRecordMetricItemModel
    {
        [Required(ErrorMessage = "RecordId is required.")]
        public int RecordId { get; set; }

        [Required(ErrorMessage = "MetricId is required.")]
        public int MetricId { get; set; }

        [Required(ErrorMessage = "Value is required.")]
        [StringLength(500, ErrorMessage = "Value cannot exceed 500 characters.")]
        public string Value { get; set; }

        [Required(ErrorMessage = "Type is required.")]
        [StringLength(100, ErrorMessage = "Type cannot exceed 100 characters.")]
        public string Type { get; set; }
    }
}
