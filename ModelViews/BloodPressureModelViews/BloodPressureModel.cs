using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.BloodPressureModelViews
{
    public class BloodPressureModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal SystolicMin { get; set; }
        public decimal SystolicMax { get; set; }
        public decimal DiastolicMin { get; set; }
        public decimal DiastolicMax { get; set; }
    }
}
