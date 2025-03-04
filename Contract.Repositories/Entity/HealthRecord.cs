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
    public class HealthRecord : BaseEntity
    {
        [Key]
        public int Id { get; set; }  // ID tự động tăng
        [Required]
        public int? PatientId { get; set; }
        public int? BandId { get; set; }
        public string? GhiChu { get; set; }
        [ForeignKey("PatientId")]
        public virtual Account? Patient { get; set; }
        public virtual Band? Band { get; set; }
        public virtual ICollection<RecordMetricItem> RecordMetricItems { get; set; }
    }
}
