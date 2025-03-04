using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.HealthRecordModelViews
{
    public class UpdateHealthRecordModel
    {
        public int? PatientId { get; set; } // Không bắt buộc khi cập nhật
        public int? BandId { get; set; } // Không bắt buộc khi cập nhật
        public string? GhiChu { get; set; } // Không bắt buộc khi cập nhật
    }
}
