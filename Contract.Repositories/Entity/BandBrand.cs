using Core.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Contract.Repositories.Entity
{
    public class BandBrand : BaseEntity
    {
        public int Id { get; set; }
        public string? NameBrand { get; set; }
        [JsonIgnore]
        public virtual ICollection<Band> Bands { get; set; }
    }
}
