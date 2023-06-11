using System.ComponentModel.DataAnnotations;

namespace iMap.ViewModels
{
    public class PasswordChangeViewModel
    {
        [Display(Name ="Your Old Password")]
        [Required(ErrorMessage = "This section is required.")]
        [DataType(DataType.Password)]
        public string PasswordOld { get; set; }

        [Display(Name = "Your New Password")]
        [Required(ErrorMessage = "This section is required.")]
        [DataType(DataType.Password)]
        [RegularExpression("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*[#$^+=!*()@%&]).{6,}$", ErrorMessage = "Minimum length 6 and must contain  1 Uppercase,1 lowercase, 1 special character and 1 digit")]
        public string PasswordNew { get; set; }

        [Display(Name = "Confirm Your Password")]
        [Required(ErrorMessage = "This section is required.")]
        [DataType(DataType.Password)]
        [Compare("PasswordNew",ErrorMessage = "Your new password and your confirmation password do not match.")] 
        public string PasswordConfirm { get; set; }
    }
}
