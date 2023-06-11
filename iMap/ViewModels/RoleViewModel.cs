using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations;

namespace iMap.ViewModels
{
    public class RoleViewModel
    {
        [Display(Name = "Role")]
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = " Role is required")]
        public string Name { get; set; }

        public string Id { set; get; }
    }
}
