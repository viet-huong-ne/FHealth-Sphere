using Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Repositories.Entity
{
    public class Metric : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Unit { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinValue { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxValue { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? DefaultValue { get; set; }

        public virtual MetricGroup? MetricGroup { get; set; }
    }
}
