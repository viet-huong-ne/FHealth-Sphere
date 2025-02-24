using Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Repositories.Entity
{
    public class Band : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int BandBrandId { get; set; }
        public string Image { get; set; }

        public virtual Account Patient { get; set; }
        public virtual BandBrand BandBrand { get; set; }

        public virtual ICollection<HealthRecord> HealthRecords { get; set; }
    }
}
