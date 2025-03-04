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
    public class Watcher : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Relative")]
        public int RelativeId { get; set; }
        [ForeignKey("Patient")]
        public int PatientId { get; set; }

        public virtual Account? Relative { get; set; }
        public virtual Account? Patient { get; set; }

        public virtual ICollection<NotificationWatcher>? NotificationWatchers { get; set; }


    }
}
