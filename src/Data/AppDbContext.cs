using Microsoft.EntityFrameworkCore;
using Yorit.Api.Data.Models;

namespace Yorit.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<ImageSource> ImageSources { get; set; }
    }
}
