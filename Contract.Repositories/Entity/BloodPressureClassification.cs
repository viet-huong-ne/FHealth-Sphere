using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Base;

namespace Contract.Repositories.Entity
{
    public class BloodPressureClassification : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public string? Name { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SystolicMin { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SystolicMax { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiastolicMin { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiastolicMax { get; set; }
    }
}
