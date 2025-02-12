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
    public class NotificationWatcher : BaseEntity
    {
        [Key]
        public int ID { get; set; }  // PK tự động tăng

        [Required]
        public int WatcherID { get; set; }  // FK -> Watcher

        [Required]
        public bool Status { get; set; }

        public DateTime Time { get; set; }

        [ForeignKey("WatcherID")]
        public virtual Watcher Watcher { get; set; }

        [ForeignKey("NotificationID")]
        public virtual NotificationSystem NotificationSystem { get; set; }
    }
}
