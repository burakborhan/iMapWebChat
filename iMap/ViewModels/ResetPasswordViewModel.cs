using System.ComponentModel.DataAnnotations;

namespace iMap.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Display(Name = "Your e-mail")]
        [Required(ErrorMessage = "E-mail is required")]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Your new password")]
        [Required(ErrorMessage = " Password is required")]
        [DataType(DataType.Password)]
        [RegularExpression("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*[#$^+=!*()@%&]).{6,}$", ErrorMessage = "Minimum length 6 and must contain  1 Uppercase,1 lowercase, 1 special character and 1 digit")]
        public string NewPassword { get; set; }
    }


}
