using System.ComponentModel.DataAnnotations;

namespace iMap.ViewModels
{
    public class RoomViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 0)]
        [RegularExpression(@"^\w+( \w+)*$", ErrorMessage = "Characters allowed: letters, numbers, and one space between words.")]
        public string? Name { get; set; }

        public string? Admin { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsPublic { get; set; }
        public string? Image { get; set; }
    }
}
