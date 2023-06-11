using System.ComponentModel.DataAnnotations;

namespace iMap.ViewModels
{
    public class MessageViewModel
    {
        public int Id { get; set; }
        [Required]
        public string? Content { get; set; }
        public DateTime Timestamp { get; set; }
        public string? FromUserName { get; set; }
        [Required]
        public string? Room { get; set; }

        public bool IsPrivate { get; set; } = false;
        public string? ToUserName { get; set; }
    }
}
