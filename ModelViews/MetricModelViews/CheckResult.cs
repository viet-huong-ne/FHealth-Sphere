using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.MetricModelViews
{
    public class CheckResult
    {
        public string MetricName { get; set; }      
        public decimal CurrentValue { get; set; }      
        public string Status { get; set; }
    }
}
