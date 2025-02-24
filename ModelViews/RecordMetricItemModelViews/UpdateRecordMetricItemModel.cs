using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.RecordMetricItemModelViews
{
    public class UpdateRecordMetricItemModel
    {
        public int? RecordId { get; set; } // Không bắt buộc khi cập nhật
        public int? MetricId { get; set; } // Không bắt buộc khi cập nhật
        public string? Value { get; set; } // Không bắt buộc khi cập nhật
        public string? Type { get; set; } // Không bắt buộc khi cập nhật
    }
}
