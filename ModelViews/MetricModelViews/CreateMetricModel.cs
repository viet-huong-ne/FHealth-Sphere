using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.MetricModelViews
{

    public class CreateMetricModel
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(255, ErrorMessage = "Name cannot exceed 255 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Unit is required.")]
        [StringLength(50, ErrorMessage = "Unit cannot exceed 50 characters.")]
        public string Unit { get; set; }

        [Required(ErrorMessage = "MinValue is required.")]
        public decimal MinValue { get; set; }

        [Required(ErrorMessage = "MaxValue is required.")]
        public decimal MaxValue { get; set; }

        [Required(ErrorMessage = "DefaultValue is required.")]
        public decimal DefaultValue { get; set; }

        public int? MetricGroupId { get; set; } // Không bắt buộc
    }
}
