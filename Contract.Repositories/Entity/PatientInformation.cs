    using Core.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Repositories.Entity
{
    public class PatientInformation : BaseEntity
    {
        public int ID { get; set; }
        public int AccountID { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public virtual Account Account { get; set; }
    }
}
