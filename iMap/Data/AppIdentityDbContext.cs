using iMap.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace iMap.Data
{
    public class AppIdentityDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public DbSet<AppRole>? AppRoles { get; set; }
        public DbSet<Room>? Rooms { get; set; }
        public DbSet<Message>? Messages { get; set; }
        public DbSet<AppUser>? AppUsers { get; set; }
        public DbSet<MyLocation>? Locations { get; set; }
        public DbSet<RoomPermission>? RoomPermissions { get; set; }
        public DbSet<BannedUser>? BannedUsers { get; set; }
        public DbSet<RoomJoinRequest>? RoomJoinRequests { get; set; }
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

    }
}
