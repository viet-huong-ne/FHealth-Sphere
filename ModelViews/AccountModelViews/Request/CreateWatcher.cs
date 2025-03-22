using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.AccountModelViews.Request
{
    public class CreateWatcher
    {
        public int RelativeId { get; set; }
        public int PatientId { get; set; }
    }
}
