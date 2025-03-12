using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.MetricModelViews
{


    public class UpdateMetricModel
    {
        public string? Name { get; set; } // Không bắt buộc khi cập nhật
        public string? Unit { get; set; } // Không bắt buộc khi cập nhật
        public decimal? MinValue { get; set; } // Không bắt buộc khi cập nhật
        public decimal? MaxValue { get; set; } // Không bắt buộc khi cập nhật
        public decimal? DefaultValue { get; set; } // Không bắt buộc khi cập nhật
        public int? MetricGroupId { get; set; } // Không bắt buộc khi cập nhật
    }
}
