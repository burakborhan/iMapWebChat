using AutoMapper;
using iMap.Data;
using iMap.Models;
using iMap.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using iMap.Helpers;

namespace iMap.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public readonly static List<UserViewModel> _Connections = new List<UserViewModel>();
        private readonly static Dictionary<string, string> _ConnectionsMap = new Dictionary<string, string>();

        private readonly AppIdentityDbContext _context;
        private readonly IMapper _mapper;

        public ChatHub(AppIdentityDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task SendPrivate(string receiverName, string message, bool isPrivate = false)
        {
            if (_ConnectionsMap.TryGetValue(receiverName, out string userId))
            {
                // Who is the sender;
                var sender = _Connections.Where(u => u.UserName == IdentityName).First();

                if (!string.IsNullOrEmpty(message.Trim()))
                {
                    // Build the message
                    var messageViewModel = new MessageViewModel()
                    {
                        Content = BasicEmojis.ParseEmojis(message),
                        FromUserName = sender.UserName,
                        Room = "",
                        Timestamp = DateTime.Now,
                        IsPrivate = isPrivate
                    };

                    // Send the message
                    await Clients.Client(userId).SendAsync("newMessage", messageViewModel);
                    await Clients.Caller.SendAsync("newMessage", messageViewModel);
                }
            }
        }

        public async Task BanUser(string receiverName, int roomId)
        {
            if (_ConnectionsMap.TryGetValue(receiverName, out string userId))
            {
                await Clients.Client(userId).SendAsync("banUser", roomId);
            }
        }

        public async Task Join(string roomName)
        {
            try
            {
                var user = _Connections.Where(u => u.UserName == IdentityName).FirstOrDefault();
                if (user != null && user.CurrentRoom != roomName)
                {
                    // Remove user from others list
                    if (!string.IsNullOrEmpty(user.CurrentRoom))
                        await Clients.OthersInGroup(user.CurrentRoom).SendAsync("removeUser", user);

                    // Join to new chat room
                    await Leave(user.CurrentRoom);
                    await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
                    user.CurrentRoom = roomName;

                    // Tell others to update their list of users
                    await Clients.OthersInGroup(roomName).SendAsync("addUser", user);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("onError", "You failed to join the chat room!" + ex.Message);
            }
        }

        public async Task Leave(string roomName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        }

        public IEnumerable<UserViewModel> GetUsers(string roomName)
        {
            return _Connections.Where(u => u.CurrentRoom == roomName).ToList();
        }

        public override Task OnConnectedAsync()
        {
            try
            {
                var user = _context.Users.Where(u => u.UserName == IdentityName).FirstOrDefault();
                var userViewModel = _mapper.Map<AppUser, UserViewModel>(user);
                userViewModel.Device = GetDevice();
                userViewModel.CurrentRoom = "";

                if (!_Connections.Any(u => u.UserName == IdentityName))
                {
                    _Connections.Add(userViewModel);
                    _ConnectionsMap.Add(IdentityName, Context.ConnectionId);
                }

                Clients.Caller.SendAsync("getProfileInfo", userViewModel);
            }
            catch (Exception ex)
            {
                Clients.Caller.SendAsync("onError", "OnConnected:" + ex.Message);
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var userConnections = _Connections.Where(u => u.UserName == IdentityName);
                var user = userConnections.FirstOrDefault();

                userConnections.ToList().ForEach(x => _Connections.Remove(x));

                // Tell other users to remove you from their list
                Clients.OthersInGroup(user.CurrentRoom).SendAsync("removeUser", user);

                // Remove mapping
                _ConnectionsMap.Remove(user.UserName);
            }
            catch (Exception ex)
            {
                Clients.Caller.SendAsync("onError", "OnDisconnected: " + ex.Message);
            }

            return base.OnDisconnectedAsync(exception);
        }

        private string IdentityName
        {
            get { return Context.User.Identity.Name; }
        }

        private string GetDevice()
        {
            var device = Context.GetHttpContext().Request.Headers["Device"].ToString();
            if (!string.IsNullOrEmpty(device) && (device.Equals("Desktop") || device.Equals("Mobile")))
                return device;

            return "Web";
        }
    }
}
// using AutoMapper;
// using iMap.Data;
// using iMap.Models;
// using iMap.ViewModels;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.SignalR;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text.RegularExpressions;
// using System.Threading.Tasks;
//
// namespace iMap.Hubs
// {
//     [Authorize]
//     public class ChatHub : Hub
//     {
//         public readonly static List<UserViewModel> _Connections = new List<UserViewModel>();
//
//         private readonly static Dictionary<string, List<string>> _ConnectionsMap =
//             new Dictionary<string, List<string>>();
//
//         private static string userConnectionMapLocker = string.Empty;
//
//         private readonly AppIdentityDbContext _context;
//         private readonly IMapper _mapper;
//
//         public ChatHub(AppIdentityDbContext context, IMapper mapper)
//         {
//             _context = context;
//             _mapper = mapper;
//         }
//
//         public async Task SendPrivate(string receiverName, string message, bool isPrivate = false)
//         {
//             var userConnections = GetUserConnections(receiverName);
//             var sender = _Connections.Where(u => u.UserName == IdentityName).First();
//
//             if (!string.IsNullOrEmpty(message.Trim()))
//             {
//                 var messageViewModel = new MessageViewModel()
//                 {
//                     Content = Regex.Replace(message, @"<.*?>", string.Empty),
//                     FromUserName = sender.UserName,
//                     Room = "",
//                     Timestamp = DateTime.Now,
//                     IsPrivate = isPrivate
//                 };
//
//                 foreach (var userConnection in userConnections)
//                 {
//                     await Clients.Client(userConnection).SendAsync("newMessage", messageViewModel);
//                 }
//                 
//                 await Clients.Caller.SendAsync("newMessage", messageViewModel);
//             }
//
//             // if (_ConnectionsMap.TryGetValue(receiverName, out string userId))
//             // {
//             //     // Who is the sender;
//             //     var sender = _Connections.Where(u => u.UserName == IdentityName).First();
//             //
//             //     if (!string.IsNullOrEmpty(message.Trim()))
//             //     {
//             //         // Build the message
//             //         var messageViewModel = new MessageViewModel()
//             //         {
//             //             Content = Regex.Replace(message, @"<.*?>", string.Empty),
//             //             FromUserName = sender.UserName,
//             //             Room = "",
//             //             Timestamp = DateTime.Now,
//             //             IsPrivate = isPrivate
//             //         };
//             //
//             //         // Send the message
//             //         await Clients.Client(userId).SendAsync("newMessage", messageViewModel);
//             //         await Clients.Caller.SendAsync("newMessage", messageViewModel);
//             //     }
//             // }
//         }
//
//         public async Task BanUser(string receiverName, int roomId)
//         {
//             var userConnections = GetUserConnections(receiverName);
//             foreach (var userConnection in userConnections)
//             {
//                 await Clients.Client(userConnection).SendAsync("banUser", roomId);
//             }
//
//             // if (_ConnectionsMap.TryGetValue(receiverName, out string userId))
//             // {
//             //     await Clients.Client(userId).SendAsync("banUser", roomId);
//             // }
//         }
//
//         public async Task Join(string roomName)
//         {
//             try
//             {
//                 var user = _Connections.Where(u => u.UserName == IdentityName).FirstOrDefault();
//                 if (user != null && user.CurrentRoom != roomName)
//                 {
//                     // Remove user from others list
//                     if (!string.IsNullOrEmpty(user.CurrentRoom))
//                         await Clients.OthersInGroup(user.CurrentRoom).SendAsync("removeUser", user);
//
//                     // Join to new chat room
//                     await Leave(user.CurrentRoom);
//                     await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
//                     user.CurrentRoom = roomName;
//
//                     // Tell others to update their list of users
//                     await Clients.OthersInGroup(roomName).SendAsync("addUser", user);
//                 }
//             }
//             catch (Exception ex)
//             {
//                 await Clients.Caller.SendAsync("onError", "You failed to join the chat room!" + ex.Message);
//             }
//         }
//
//         public async Task Leave(string roomName)
//         {
//             await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
//         }
//
//         public IEnumerable<UserViewModel> GetUsers(string roomName)
//         {
//             return _Connections.Where(u => u.CurrentRoom == roomName).ToList();
//         }
//
//         public override Task OnConnectedAsync()
//         {
//             try
//             {
//                 var user = _context.Users.Where(u => u.UserName == IdentityName).FirstOrDefault();
//                 var userViewModel = _mapper.Map<AppUser, UserViewModel>(user);
//                 userViewModel.Device = GetDevice();
//                 userViewModel.CurrentRoom = "";
//
//                 _Connections.Add(userViewModel);
//                 KeepUserConnection(IdentityName, Context.ConnectionId);
//                 // _ConnectionsMap.Add(IdentityName, Context.ConnectionId);
//
//                 Clients.Caller.SendAsync("getProfileInfo", userViewModel);
//             }
//             catch (Exception ex)
//             {
//                 Clients.Caller.SendAsync("onError", "OnConnected:" + ex.Message);
//             }
//
//             return base.OnConnectedAsync();
//         }
//
//         public override Task OnDisconnectedAsync(Exception exception)
//         {
//             try
//             {
//                 var userConnections = _Connections.Where(u => u.UserName == IdentityName);
//                 var user = userConnections.FirstOrDefault();
//
//                 var connectionId = Context.ConnectionId;
//                RemoveUserConnection(connectionId);
//
//                 // Tell other users to remove you from their list
//                 Clients.OthersInGroup(user.CurrentRoom).SendAsync("removeUser", user);
//
//                 // Remove mapping
//                 // _ConnectionsMap.Remove(user.UserName);
//             }
//             catch (Exception ex)
//             {
//                 Clients.Caller.SendAsync("onError", "OnDisconnected: " + ex.Message);
//             }
//
//             return base.OnDisconnectedAsync(exception);
//         }
//
//         private string IdentityName
//         {
//             get { return Context.User.Identity.Name; }
//         }
//
//         private string GetDevice()
//         {
//             var device = Context.GetHttpContext().Request.Headers["Device"].ToString();
//             if (!string.IsNullOrEmpty(device) && (device.Equals("Desktop") || device.Equals("Mobile")))
//                 return device;
//
//             return "Web";
//         }
//
//         public void KeepUserConnection(string userId, string connectionId)
//         {
//             lock (userConnectionMapLocker)
//             {
//                 if (!_ConnectionsMap.ContainsKey(userId))
//                 {
//                     _ConnectionsMap[userId] = new List<string>();
//                 }
//
//                 _ConnectionsMap[userId].Add(connectionId);
//             }
//         }
//
//         public void RemoveUserConnection(string connectionId)
//         {
//             //This method will remove the connectionId of user
//             lock (userConnectionMapLocker)
//             {
//                 foreach (var userId in _ConnectionsMap.Keys)
//                 {
//                     if (_ConnectionsMap.ContainsKey(userId))
//                     {
//                         if (_ConnectionsMap[userId].Contains(connectionId))
//                         {
//                             _ConnectionsMap[userId].Remove(connectionId);
//                             break;
//                         }
//                     }
//                 }
//             }
//         }
//
//         public List<string> GetUserConnections(string userId)
//         {
//             var conn = new List<string>();
//             lock (userConnectionMapLocker)
//             {
//                 if (_ConnectionsMap.ContainsKey(userId))
//                 {
//                     conn = _ConnectionsMap[userId];
//                 }
//             }
//
//             return conn;
//         }
//     }
// }
