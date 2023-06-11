using iMap.Data;
using iMap.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace iMap.Controllers
{
    public class AdminController : BaseController
    {


        public AdminController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager, AppIdentityDbContext context) : base(userManager, signInManager,roleManager,context )
        {

        }

        public IActionResult Index()
        {
            return View(userManager.Users.ToList());
        }

        public IActionResult RoleCreate()
        {
            return View();
        }

    }
}
