using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.AuthModelViews
{
    public class AddRoleModel
    {
        [Required]
        public int UserId { get; set; } 

        [Required]
        public string RoleName { get; set; }
    }
}
