using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iMap.Data;
using iMap.Models;
using iMap.Hubs;
using Microsoft.AspNetCore.SignalR;
using iMap.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Identity;

namespace iMap.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class MemberController : BaseController
    {
        private readonly AppIdentityDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;


        public MemberController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            RoleManager<AppRole> roleManager, AppIdentityDbContext context, IHubContext<ChatHub> hubContext) : base(userManager, signInManager,
            roleManager, context)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult Index(double latitude, double longitude)
        {
            AppUser user = CurrentUser;
            UserViewModel userViewModel = user.Adapt<UserViewModel>();

            var myLocation = new MyLocation();

            return View(userViewModel);
        }

        [Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult PasswordChange()
        {
            return View();
        }

        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult PasswordChange(PasswordChangeViewModel passwordChangeViewModel)
        {
            if (ModelState.IsValid)
            {
                AppUser user = CurrentUser;

                bool exist = userManager.CheckPasswordAsync(user, passwordChangeViewModel.PasswordOld).Result;

                if (exist)
                {
                    IdentityResult result = userManager.ChangePasswordAsync(user, passwordChangeViewModel.PasswordOld,
                        passwordChangeViewModel.PasswordNew).Result;

                    if (result.Succeeded)
                    {
                        userManager.UpdateSecurityStampAsync(user);

                        signInManager.SignOutAsync();
                        signInManager.PasswordSignInAsync(user, passwordChangeViewModel.PasswordNew, true, false);

                        ViewBag.success = "true";
                    }
                    else
                    {
                        AddModelError(result);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Your old password is wrong.");
                }
            }

            return View(passwordChangeViewModel);
        }

        public async void LogOut()
        {
            await signInManager.SignOutAsync();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult ChatPopupViewClosed()
        {
            return View();
        }
        
        public async Task<IActionResult> ChatPopupView(int roomId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name)
                .ConfigureAwait(false);
            var room = await _context.Rooms.Include(r => r.Admin).FirstOrDefaultAsync(x => x.Id == roomId)
                .ConfigureAwait(false);
            if (DateTime.Now>room.ExpiringAt)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("removeChatRoom", room.Id);
                await _hubContext.Clients.Group(room.Name).SendAsync("onRoomDeleted");
                return RedirectToAction("ChatPopupViewClosed");
            }
            var checkUserIfBanned =
                await _context.BannedUsers.FirstOrDefaultAsync(x => x.UserId == user.Id && x.RoomId == roomId).ConfigureAwait(false);
            if (checkUserIfBanned!=null)
            {
                return RedirectToAction("ChatPopupViewBanned", "Member");
            }
            if (!room.IsPublic)
            {
                if (room.Admin.UserName != user.UserName)
                {
                    var checkUserPermission =
                        _context.RoomPermissions.FirstOrDefault(x => x.UserId == user.Id && x.RoomId == roomId);
                    if (checkUserPermission == null)
                    {
                        var checkUserRoomJoinRequest =
                            _context.RoomJoinRequests.FirstOrDefault(x => x.UserId == user.Id && x.RoomId == roomId);
                        if (checkUserRoomJoinRequest == null)
                        {
                            var roomJoinRequest = new RoomJoinRequest
                            {
                                UserId = user.Id,
                                RoomId = roomId
                            };
                            await _context.RoomJoinRequests.AddAsync(roomJoinRequest).ConfigureAwait(false);
                            await _context.SaveChangesAsync().ConfigureAwait(false);
                        }

                        return RedirectToAction("ChatPopupViewNotAuthorized", "Member", roomId);
                    }
                }
            }

            return View(roomId);
        }

        public async Task<IActionResult> ChatPopupDetailView(string username,int roomId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name)
                .ConfigureAwait(false);
            var room = await _context.Rooms.Include(r => r.Admin).FirstOrDefaultAsync(x => x.Id == roomId)
                .ConfigureAwait(false);
            
            await _hubContext.Clients.Group(room.Name).SendAsync("removeUser",user);
            
            var chatPopupDetailViewModel = new ChatPopupDetailViewModel
            {
                Username = username,
                RoomId = roomId
            };
            return View(chatPopupDetailViewModel);
        }

        public IActionResult ChatPopupViewNotAuthorized()
        {
            return View();
        }
        
        public IActionResult ChatPopupViewBanned()
        {
            return View();
        }

        public async Task<IActionResult> RoomJoinRequests(int roomId)
        {
            var joinRequests = await _context.RoomJoinRequests.Where(x => x.RoomId == roomId).ToListAsync();
            var joinRequestModels = new List<RoomJoinRequestModel>();
            foreach (var joinRequest in joinRequests)
            {
                var joinRequestUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == joinRequest.UserId).ConfigureAwait(false);
                joinRequestModels.Add(new RoomJoinRequestModel
                {
                    UserId = joinRequest.UserId,
                    UserName = joinRequestUser.UserName,
                    Id = joinRequest.Id,
                    RoomId = joinRequest.RoomId
                });
            }
            
            return View(joinRequestModels);
        }

        [HttpPost]
        public async Task ApproveRoomJoinRequest(int id)
        {
            var roomJoinRequestToDelete =
                await _context.RoomJoinRequests.FirstOrDefaultAsync(x => x.Id == id).ConfigureAwait(false);
            var roomPermission = new RoomPermission
            {
                UserId = roomJoinRequestToDelete.UserId,
                RoomId = roomJoinRequestToDelete.RoomId
            };
            _context.RoomJoinRequests.Remove(roomJoinRequestToDelete);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            await _context.RoomPermissions.AddAsync(roomPermission).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
        [HttpPost]
        public async Task DeclineRoomJoinRequest(int id)
        {
            var roomJoinRequestToDelete =
                await _context.RoomJoinRequests.FirstOrDefaultAsync(x => x.Id == id).ConfigureAwait(false);
            _context.RoomJoinRequests.Remove(roomJoinRequestToDelete);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}