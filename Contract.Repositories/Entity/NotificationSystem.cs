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

        public int ID { get; set; }
        [Required]
        public int AccountID { get; set; }
        public string Content { get; set; }
        public string status { get; set; }
        [ForeignKey("AccountID")]
        public virtual Account Accounts { get; set; } 
        public virtual ICollection<NotificationWatcher> NotificationWatchers { get; set; }


    }
}
