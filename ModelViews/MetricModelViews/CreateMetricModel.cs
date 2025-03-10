using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.MetricModelViews
{
    public class ValidateMetricRangeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (CreateMetricModel)validationContext.ObjectInstance;

            if (model.MinValue > model.DefaultValue)
            {
                return new ValidationResult("DefaultValue must be greater than or equal to MinValue.");
            }

            if (model.DefaultValue > model.MaxValue)
            {
                return new ValidationResult("DefaultValue must be less than or equal to MaxValue.");
            }

            if (model.MinValue > model.MaxValue)
            {
                return new ValidationResult("MinValue must be less than or equal to MaxValue.");
            }

            return ValidationResult.Success;
        }
    }

    [ValidateMetricRange]
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
