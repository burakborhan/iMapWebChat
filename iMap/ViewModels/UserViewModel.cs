using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Build.Framework;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Web;

namespace iMap.ViewModels
{
    public class UserViewModel
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "User name is required.")]
        [Display(Name ="User Name")]
        public string UserName { get; set; }


        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^(0(\d{3}) (\d{3}) (\d{2}) (\d{2}))$", ErrorMessage ="Wrong typing : xxxx xxx xx xx")]
        [Display(Name = "Phone Number")]
        [Phone]
        public string PhoneNumber { get; set; }


        [Display(Name = "E-mail")]
        [EmailAddress]
        public string Email { get; set; }


        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Password is required.")]
        [Display(Name = "Password")]
        [RegularExpression("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*[#$^+=!*()@%&]).{6,}$", ErrorMessage = "Minimum length 6 and must contain  1 Uppercase,1 lowercase, 1 special character and 1 digit")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string? CurrentRoom { get; set; }
        public string? Device { get; set; }

    }
}
