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
        public int ID { get; set; }  // ID tự động tăng
        [Required]
        public int PatientID { get; set; }
        public int BandID { get; set; }
        public string GhiChu { get; set; }
        [ForeignKey("PatientID")]
        public virtual Account Patient { get; set; }
        public virtual Band Band { get; set; }
        public virtual ICollection<RecordMetricItem> RecordMetricItems { get; set; }
    }
}
