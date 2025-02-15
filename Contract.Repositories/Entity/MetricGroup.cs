using Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Repositories.Entity
{
    public class MetricGroup : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public int DisplayOrder {  get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public virtual ICollection<Metric> Tags { get; set; }
    }
}
