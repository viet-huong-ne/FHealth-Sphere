using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.BandModelViews
{
    public class CreateBandModel
    {
        [Required(ErrorMessage = "PatientId is required.")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "BandBrandId is required.")]
        public int BandBrandId { get; set; }

        [Required(ErrorMessage = "Image is required.")]
        [StringLength(500, ErrorMessage = "Image cannot exceed 500 characters.")]
        public string Image { get; set; }
        [Required(ErrorMessage = "BandCode is required.")]
        public string BandCode { get; set; }
    }
}
