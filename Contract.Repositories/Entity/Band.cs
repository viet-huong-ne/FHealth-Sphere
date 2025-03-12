using Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Contract.Repositories.Entity
{
    public class Band : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string? Image { get; set; }

        public string? BandCode { get; set; }
        [JsonIgnore]
        public virtual Account Patient { get; set; }
        [JsonIgnore]
        public virtual BandBrand? BandBrand { get; set; }
        [JsonIgnore]
        public virtual ICollection<HealthRecord> HealthRecords { get; set; }
    }
}
