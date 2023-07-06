using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iMap.Data;
using iMap.Models;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using iMap.Helper;
using iMap.Hubs;
using Microsoft.AspNetCore.SignalR;
using iMap.ViewModels;
using System.Text.Json;

namespace iMap.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly AppIdentityDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IUploadHelper _uploadHelper;

        public RoomsController(AppIdentityDbContext context,
            IMapper mapper,
            IHubContext<ChatHub> hubContext, IUploadHelper uploadHelper)
        {
            _context = context;
            _mapper = mapper;
            _hubContext = hubContext;
            _uploadHelper = uploadHelper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomViewModel>>> Get()
        {
            var rooms = await _context.Rooms
                .Include(r => r.Admin)
                .ToListAsync(); 

            var roomsViewModel = _mapper.Map<IEnumerable<Room>, IEnumerable<RoomViewModel>>(rooms);

            return Ok(roomsViewModel);
        }
        

        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> Get(int id)
        {
            var room = await _context.Rooms.Include(r=>r.Admin).FirstOrDefaultAsync(x=>x.Id==id);
            var user = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            var checkUserIfBanned =
                await _context.BannedUsers.FirstOrDefaultAsync(x => x.UserId == user.Id && x.RoomId == room.Id).ConfigureAwait(false);
            if (checkUserIfBanned!=null)
            {
                return new Room
                {
                    Id = -1
                };
            }
            if (room == null)
                return NotFound();

            var roomViewModel = _mapper.Map<Room, RoomViewModel>(room);
            return Ok(roomViewModel);
        }

        [HttpGet("ClearBans")]
        public async Task ClearBans()
        {
            var bans = await _context.BannedUsers.ToListAsync();
            _context.BannedUsers.RemoveRange(bans);
            await _context.SaveChangesAsync();
        }

        [HttpPost("Create")]
        public async Task<ActionResult<Room>> Create(RoomViewModel viewModel)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            var checkUserHasAlreadyCreatedRoom = _context.Rooms.Any(x => x.Admin.Id == user.Id);
            if (checkUserHasAlreadyCreatedRoom)
            {
                return BadRequest(JsonSerializer.Serialize(new ErrorJsonResult
                { Status = 400, Message = "User exceeded room create limits!" }));
            }

            //if(viewModel.Name== "" || viewModel.Name.Length==0) 
            //{
            //    return BadRequest(JsonSerializer.Serialize(new ErrorJsonResult
            //    { Status = 400, Message = "Room name can not be empty!" }));
            //}

            if (_context.Rooms.Any(r => r.Name == viewModel.Name))
                return BadRequest(JsonSerializer.Serialize(new ErrorJsonResult
                { Status = 400, Message = "Invalid room name or room already exists!" }));
            
            if(viewModel.Image == "")
                return BadRequest(JsonSerializer.Serialize(new ErrorJsonResult
                { Status = 400, Message = "Room icon is necessary!" }));

            var room = new Room()
            {
                Name = viewModel.Name,
                Admin = user,
                IsPublic = viewModel.IsPublic,
                Longitude = viewModel.Longitude,
                Latitude = viewModel.Latitude,
                Image = viewModel.Image
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            
            var createdRoom = _mapper.Map<Room, RoomViewModel>(room);
            await _hubContext.Clients.All.SendAsync("addChatRoom", createdRoom);

            return CreatedAtAction(nameof(Get), new { id = room.Id }, createdRoom);
        }

        [HttpPost("UploadImage")]
        public async Task<string> UploadImage(IFormFile file)
        {
            string path = "";
            if (file!=null)
            {
                path = await _uploadHelper.UploadImage(file);
            }

            return path;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, RoomViewModel viewModel)
        {
            var room = await _context.Rooms
                .Include(r => r.Admin)
                .Where(r => r.Id == id && r.Admin.UserName == User.Identity.Name)
                .FirstOrDefaultAsync();

            if (room == null)
                return NotFound();

            room.Name = viewModel.Name;
            room.IsPublic = viewModel.IsPublic;
            await _context.SaveChangesAsync();

            var updatedRoom = _mapper.Map<Room, RoomViewModel>(room);
            await _hubContext.Clients.All.SendAsync("updateChatRoom", updatedRoom);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.Admin)
                .Where(r => r.Id == id && r.Admin.UserName == User.Identity.Name)
                .FirstOrDefaultAsync();

            if (room == null)
                return NotFound();

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("removeChatRoom", room.Id);
            await _hubContext.Clients.Group(room.Name).SendAsync("onRoomDeleted");

            return Ok();
        }
        
        [HttpPost("BanUser")]
        public async Task BanUser(BannedUserViewModel bannedUserViewModel)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName.ToLower() == bannedUserViewModel.UserName.ToLower());
            if (user==null)
            {
                return;
            }
            var bannedUser = new BannedUser
            {
                UserId = user.Id,
                RoomId = bannedUserViewModel.RoomId
            };
            await _context.BannedUsers.AddAsync(bannedUser).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            await _hubContext.Clients.All.SendAsync("BanUser",bannedUserViewModel.RoomId,user.UserName);
        }
    }
}
