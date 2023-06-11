namespace iMap.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public AppUser Admin { get; set; }
        public ICollection<Message> Messages { get; set; }
        public string Image { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsPublic { get; set; }
        public DateTime ExpiringAt { get; set; } = DateTime.Now.AddHours(1);
    }
}
