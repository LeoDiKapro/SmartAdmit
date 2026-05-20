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
        public DbSet<ApplicationLanguage> ApplicationLanguages { get; set; }
        public DbSet<ScoringWeights> ScoringWeights { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<University>()
                .HasMany(u => u.MasterPrograms)
                .WithOne(p => p.University)
                .HasForeignKey(p => p.UniversityId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MasterProgram>()
                .HasMany(p => p.Applications)
                .WithOne(a => a.MasterProgram)
                .HasForeignKey(a => a.MasterProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed universities + programs so the dropdown has data immediately
            builder.Entity<University>().HasData(
                new University { Id = 1, Name = "University of Tirana" },
                new University { Id = 2, Name = "Polytechnic University" },
                new University { Id = 3, Name = "European University of Tirana" }
            );

            builder.Entity<MasterProgram>().HasData(
                // University of Tirana
                new MasterProgram { Id = 1, UniversityId = 1, Name = "Computer Science", MinGPA = 3.0m, MinYears = 3, Field = EducationField.ComputerScience },
                new MasterProgram { Id = 2, UniversityId = 1, Name = "Business Administration", MinGPA = 2.5m, MinYears = 3 , Field = EducationField.Business },
                new MasterProgram { Id = 3, UniversityId = 1, Name = "Law", MinGPA = 2.8m, MinYears = 4, Field = EducationField.Law },

                // Polytechnic
                new MasterProgram { Id = 4, UniversityId = 2, Name = "Software Engineering", MinGPA = 3.2m, MinYears = 3, Field = EducationField.ComputerScience },
                new MasterProgram { Id = 5, UniversityId = 2, Name = "Civil Engineering", MinGPA = 3.0m, MinYears = 3, Field = EducationField.Engineering },
                new MasterProgram { Id = 6, UniversityId = 2, Name = "Electrical Engineering", MinGPA = 3.0m, MinYears = 3, Field = EducationField.Engineering },

                // European University
                new MasterProgram { Id = 7, UniversityId = 3, Name = "International Relations", MinGPA = 2.8m, MinYears = 3, Field = EducationField.SocialSciences },
                new MasterProgram { Id = 8, UniversityId = 3, Name = "Economics", MinGPA = 2.7m, MinYears = 3, Field = EducationField.Business },
                new MasterProgram { Id = 9, UniversityId = 3, Name = "Psychology", MinGPA = 2.5m, MinYears = 3, Field = EducationField.Medicine }
            );
        }
    }
}
