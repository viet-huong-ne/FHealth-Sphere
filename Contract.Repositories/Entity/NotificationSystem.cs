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
    public class NotificationSystem : BaseEntity
    {

        public int Id { get; set; }
        [Required]
        public int? AccountId { get; set; }
        public string? Content { get; set; }
        public string? status { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account? Accounts { get; set; } 
        public virtual ICollection<NotificationWatcher>? NotificationWatchers { get; set; }


    }
}
