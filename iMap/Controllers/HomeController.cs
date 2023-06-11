
using System.Device.Location;
using iMap.Data;
using iMap.Helper;
using iMap.Models;
using iMap.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace iMap.Controllers
{
    public class HomeController : BaseController
    {



        public HomeController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager, AppIdentityDbContext context) : base(userManager, signInManager, roleManager, context)
        {

        }
        public IActionResult Login(string ReturnUrl)
        {

            TempData["ReturnUrl"] = ReturnUrl;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel userLogin)
        {
            if (ModelState.IsValid)
            {
                AppUser user = await userManager.FindByNameAsync(userLogin.userName);

                if (user != null)
                {
                    if (await userManager.IsLockedOutAsync(user))
                    {
                        ModelState.AddModelError("", "Your account is locked for a while. Please try again later.");
                        return View(userLogin);
                    }

                    if (userManager.IsEmailConfirmedAsync(user).Result == false)
                    {
                        ModelState.AddModelError("", "Your e-mail adress is not confirmed. Please confirm your e-mail adress.");
                        return View(userLogin);
                    }

                    await signInManager.SignOutAsync();
                    Microsoft.AspNetCore.Identity.SignInResult result = await signInManager.PasswordSignInAsync(user, userLogin.Password, userLogin.RememberMe, false);




                    if (result.Succeeded)
                    {
                        await userManager.ResetAccessFailedCountAsync(user);

                        if (TempData["ReturnUrl"] != null)
                        {
                            return Redirect(TempData["ReturnUrl"].ToString());
                        }
                        return RedirectToAction("Index", "Member");
                    }
                    else
                    {
                        await userManager.AccessFailedAsync(user);

                        int fail = await userManager.GetAccessFailedCountAsync(user);

                        ModelState.AddModelError("", $"{fail} failed login. ");

                        if (fail == 3)
                        {
                            await userManager.SetLockoutEndDateAsync(user, new DateTimeOffset(DateTime.Now.AddMinutes(10)));
                            ModelState.AddModelError("", "Your login attempt failed 3 times. Your account has been banned for 10 minutes. Please try again later.");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Invalid username or password");
                        }

                    }
                }
                else
                {
                    ModelState.AddModelError(nameof(LoginViewModel.userName), "Invalid username or password");
                }

            }
            return View(userLogin);
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(UserViewModel userViewModel)
        {

            if (ModelState.IsValid)
            {
                if (userManager.Users.Any(u => u.PhoneNumber == userViewModel.PhoneNumber))
                {
                    ModelState.AddModelError("", "This phone number is registered");
                    return View(userViewModel);
                }


                AppUser user = new AppUser();
                user.UserName = userViewModel.UserName;
                user.Email = userViewModel.Email;
                user.PhoneNumber = userViewModel.PhoneNumber;

                IdentityResult result = await userManager.CreateAsync(user, userViewModel.Password);

                if (result.Succeeded)
                {
                    string confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    string link = Url.Action("ConfirmEmail", "Home", new
                    {
                        userId = user.Id,
                        token = confirmationToken,
                    }, protocol: HttpContext.Request.Scheme

                    );

                    Helper.EmailConfirmation.EmailConfirmationHelper(link, user.Email);

                    return RedirectToAction("Login");
                }
                else
                {
                    AddModelError(result);
                }
            }

            return View(userViewModel);
        }
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
        {
            AppUser user = userManager.FindByEmailAsync(resetPasswordViewModel.Email).Result;

            if (user != null)
            {
                string passwordResetToken = userManager.GeneratePasswordResetTokenAsync(user).Result;

                string passwordResetLink = Url.Action("ResetPasswordConfirm", "Home", new
                {
                    userId = user.Id,
                    token = passwordResetToken
                }, HttpContext.Request.Scheme);

                //www.iMapMapComminication.com/Home/ResetPasswordConfirm?userId=asdfasdfsd&token=sadfasdfsa
                Helper.PasswordReset.PasswordResetEmail(passwordResetLink, resetPasswordViewModel);

                ViewBag.status = "Succesfull";
            }
            else
            {
                ModelState.AddModelError("", "Your e-mail address is not registered in the system.");
            }


            return View(resetPasswordViewModel);
        }

        public IActionResult ResetPasswordConfirm(string userId, string token)
        {
            TempData["userId"] = userId;
            TempData["token"] = token;

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ResetPasswordConfirm(ResetPasswordViewModel request)
        {
            var userId = TempData["userId"];
            var token = TempData["token"];

            if (userId == null || token == null)
            {
                throw new Exception("An error has occurred");
            }

            var hasUser = await userManager.FindByIdAsync(userId.ToString()!);

            if (hasUser == null)
            {
                ModelState.AddModelError(String.Empty, "User couldn't found.");
                return View();
            }

            IdentityResult result = await userManager.ResetPasswordAsync(hasUser, token.ToString()!, request.NewPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Your password has been successfully reset";
            }
            else
            {
                ModelState.AddModelError("", "There was an error. Please try again.");
            }

            return View();
        }
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await userManager.FindByIdAsync(userId);

            IdentityResult result = await userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                ViewBag.status = "Your e-mail is confirmed. You can login in login page.";
            }
            else
            {
                ViewBag.status = "An error occured. Please try again.";
            }

            return View();
        }


        public IActionResult FacebookLogin(string ReturnUrl)
        {
            string? RedirectUrl = Url.Action("ExternalResponse", "Home", new { ReturnUrl = ReturnUrl });

            var properties = signInManager.ConfigureExternalAuthenticationProperties("Facebook", RedirectUrl);

            return new ChallengeResult("Facebook", properties);
        }
        public IActionResult GoogleLogin(string ReturnUrl)
        {
            string? RedirectUrl = Url.Action("ExternalResponse", "Home", new { ReturnUrl = ReturnUrl });

            var properties = signInManager.ConfigureExternalAuthenticationProperties("Google", RedirectUrl);

            return new ChallengeResult("Google", properties);
        }

        public async Task<IActionResult> ExternalResponse(string ReturnUrl = "/")
        {
            ExternalLoginInfo info = await signInManager.GetExternalLoginInfoAsync(ReturnUrl);
            if (info == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                Microsoft.AspNetCore.Identity.SignInResult result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true);

                if (result.Succeeded)
                {
                    return Redirect(ReturnUrl);
                }
                else
                {
                    AppUser user = new AppUser();
                    user.Email = info.Principal.FindFirst(ClaimTypes.Email).Value;
                    string ExternalUserId = info.Principal.FindFirst(ClaimTypes.NameIdentifier).Value;

                    if (info.Principal.HasClaim(x => x.Type == ClaimTypes.Name))
                    {
                        string userName = info.Principal.FindFirst(ClaimTypes.Name).Value;

                        user.UserName = userName.Replace(' ', '-').ToLower() + ExternalUserId.Substring(0, 5).ToString();
                        user.UserName = userName;
                    }
                    else
                    {
                        user.UserName = info.Principal.FindFirst(ClaimTypes.Email).Value;
                    }

                    IdentityResult result1 = await userManager.CreateAsync(user);

                    if (result1.Succeeded)
                    {
                        IdentityResult loginResult = await userManager.AddLoginAsync(user, info);

                        if (loginResult.Succeeded)
                        {
                            await signInManager.SignInAsync(user, true);
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            AddModelError(loginResult);
                        }
                    }
                    else
                    {
                        AddModelError(result1);
                    }

                }
            }

            return RedirectToAction("Error");

        }

        //[HttpPost]
        //public async Task<IActionResult> SaveLocation(double latitude, double longitude)
        //{
        //    // Create a new location entity and add it to the database
        //    MyLocation location = new MyLocation { Latitude = latitude, Longitude = longitude };

        //    await context.Locations.AddAsync(location);
        //    await context.SaveChangesAsync();
        //123
        //    // Return the location data to the client-side
        //    return Json(new { latitude = location.Latitude, longitude = location.Longitude });
        //}
        [HttpPost]
        public async Task<IActionResult> GetLocation(MyLocation location)
        {
            GeoCoordinate geoCoordinate = new GeoCoordinate(location.Latitude, location.Longitude);

            MyLocation myLocation = new MyLocation
            {
                UserId = location.UserId,
                Latitude = location.Latitude,
                Longitude = location.Longitude
            };

            context.Locations.Add(myLocation);
            await context.SaveChangesAsync();

            return Json(new { latitude = myLocation.Latitude, longitude = myLocation.Longitude });
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}