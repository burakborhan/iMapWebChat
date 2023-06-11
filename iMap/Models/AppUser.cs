using Microsoft.AspNetCore.Identity;

namespace iMap.Models
{
    public class AppUser : IdentityUser
    {
        public ICollection<Room>? Rooms { get; set; }
        public ICollection<Message>? Messages { get; set; }
        public ICollection<MyLocation>? MyLocations { get; set; }


    }
}
