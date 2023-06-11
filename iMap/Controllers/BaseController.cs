using iMap.Data;
using iMap.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iMap.Controllers
{
    public class BaseController : Controller
    {
        protected UserManager<AppUser> userManager { get; set; }
        protected SignInManager<AppUser> signInManager { get; set; }
        protected RoleManager<AppRole> roleManager { get; set; }

        protected readonly AppIdentityDbContext context;

        protected AppUser CurrentUser => userManager.FindByNameAsync(User.Identity.Name).Result;


        public BaseController( UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager, AppIdentityDbContext context)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this.context = context;
        }


        public void AddModelError(IdentityResult result)
        {
            foreach (var item in result.Errors)
            {
                ModelState.AddModelError("", item.Description);
            }
        }
    }
}
