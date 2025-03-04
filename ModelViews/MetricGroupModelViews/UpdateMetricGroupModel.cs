using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.MetricGroupModelViews
{
    public class UpdateMetricGroupModel
    {
        public string? Name { get; set; } // Không bắt buộc khi cập nhật
        public int? DisplayOrder { get; set; } // Không bắt buộc khi cập nhật
        public string? Status { get; set; } // Không bắt buộc khi cập nhật
    }
}
