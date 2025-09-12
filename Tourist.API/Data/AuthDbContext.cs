using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tourist.API.Models;

namespace Tourist.API.Data
{
    public class AuthDbContext : IdentityDbContext<ApplicationUser>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Use static GUIDs
            string adminRoleId = "f4d2bdc6-0276-4547-a442-477215db9f9d";
            string contributorRoleId = "e7753234-234b-41bb-a616-5d4c3a5ea4eb";
            string readerRoleId = "c228ea71-99c4-4820-8749-077d71797007";
            string adminUserId = "30453bad-8964-4407-9747-bd8766a5cf6b";

            builder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = readerRoleId,
                    Name = "Reader",
                    NormalizedName = "READER",
                    ConcurrencyStamp = readerRoleId
                },
                new IdentityRole
                {
                    Id = contributorRoleId,
                    Name = "Contributor",
                    NormalizedName = "CONTRIBUTOR",
                    ConcurrencyStamp = contributorRoleId
                },
                new IdentityRole
                {
                    Id = adminRoleId,
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = adminRoleId
                }
            );

            // Use a static password hash (for "Admin@123")
            var admin = new ApplicationUser
            {
                Id = adminUserId,
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@tourist.com",
                NormalizedEmail = "ADMIN@TOURIST.COM",
                PasswordHash = "AQAAAAIAAYagAAAAEPYurKz2OyZD3jXYQvgr/0Jfer/9AkBgCRDegdV3Zu1I9KLnzYf92pKGmx7PbiEzhQ==",
                SecurityStamp = "546f9154-dcff-44c6-8d32-b76ee9429b70",
                ConcurrencyStamp = "fee34868-8f0d-4862-b4be-9821feb7b317"
            };

            builder.Entity<ApplicationUser>().HasData(admin);

            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    RoleId = adminRoleId,
                    UserId = adminUserId
                }
            );
        }
    }
}
