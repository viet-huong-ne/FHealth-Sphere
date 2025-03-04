using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.BandModelViews
{
    public class UpdateBandModel
    {
        public int? PatientId { get; set; } // Không bắt buộc khi cập nhật
        public int? BandBrandId { get; set; } // Không bắt buộc khi cập nhật
        public string? Image { get; set; } // Không bắt buộc khi cập nhật
    }
}
