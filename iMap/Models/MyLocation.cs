namespace iMap.Models
{
    public class MyLocation
    {
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public AppUser? UserId { get; set; }
    }
}
