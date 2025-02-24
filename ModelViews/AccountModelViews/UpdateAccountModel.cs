using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.AccountModelViews
{
    public class UpdateAccountModel
    {
        [Required(ErrorMessage = "UserName is required.")]
        [StringLength(256, ErrorMessage = "UserName cannot exceed 256 characters.")]
        public string UserName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string? PhoneNumber { get; set; } // Không bắt buộc khi cập nhật

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters if provided.")]
        public string? Password { get; set; } // Không bắt buộc khi cập nhật
    }
}
