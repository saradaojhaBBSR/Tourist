using Microsoft.EntityFrameworkCore;
using Tourist.API.Models;

namespace Tourist.API.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<TouristPlaces> TouristPlaces { get; set; }
    }
}
