using AdmissionsPortal.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdmissionsPortal.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<University> Universities { get; set; }
        public DbSet<MasterProgram> MasterPrograms { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Document> Documents { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seed universities + programs so the dropdown has data immediately
            builder.Entity<University>().HasData(
                new University { Id = 1, Name = "University of Tirana", MinGPA = 1.66m, MinYears = 3 },
                new University { Id = 2, Name = "Polytechnic University", MinGPA = 2.0m, MinYears = 3 },
                new University { Id = 3, Name = "European University of Tirana", MinGPA = 2.33m, MinYears = 3 }
            );

            builder.Entity<MasterProgram>().HasData(
                // University of Tirana
                new MasterProgram { Id = 1, UniversityId = 1, Name = "Computer Science" },
                new MasterProgram { Id = 2, UniversityId = 1, Name = "Business Administration" },
                new MasterProgram { Id = 3, UniversityId = 1, Name = "Law" },

                // Polytechnic
                new MasterProgram { Id = 4, UniversityId = 2, Name = "Software Engineering" },
                new MasterProgram { Id = 5, UniversityId = 2, Name = "Civil Engineering" },
                new MasterProgram { Id = 6, UniversityId = 2, Name = "Electrical Engineering" },

                // European University
                new MasterProgram { Id = 7, UniversityId = 3, Name = "International Relations" },
                new MasterProgram { Id = 8, UniversityId = 3, Name = "Economics" },
                new MasterProgram { Id = 9, UniversityId = 3, Name = "Psychology" }
            );
        }
    }
}
