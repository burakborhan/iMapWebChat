
using System.ComponentModel.DataAnnotations;

namespace iMap.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage =" Username is required")]
        [Display(Name ="Username")]
        public string userName { get; set; }


        [Required(ErrorMessage = " Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }

    }
}
