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
        [Required(ErrorMessage = "PatientId is required.")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "BandId is required.")]
        public int BandId { get; set; }

        [Required(ErrorMessage = "GhiChu is required.")]
        [StringLength(1000, ErrorMessage = "GhiChu cannot exceed 1000 characters.")]
        public string GhiChu { get; set; }
    }
}
