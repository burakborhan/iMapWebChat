using Microsoft.Build.Framework;
using Microsoft.VisualBasic;

namespace iMap.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public DateTime Timestamp { get; set; }
        public AppUser? FromUser { get; set; }
        public int ToRoomId { get; set; }
        public Room? ToRoom { get; set; }
        public string? ToUserName { get; set; }
    }
}
