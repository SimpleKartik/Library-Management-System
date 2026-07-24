using LibraryManagement.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Web.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Librarian> Librarians { get; set; }
        public DbSet<BorrowRecord> BorrowRecords { get; set; }
        public DbSet<Login> Logins { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fluent API Configurations

            // 1-to-1 relationship between Login and Student
            modelBuilder.Entity<Student>()
                .HasOne(s => s.Login)
                .WithOne(l => l.Student)
                .HasForeignKey<Student>(s => s.LoginId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1-to-1 relationship between Login and Librarian
            modelBuilder.Entity<Librarian>()
                .HasOne(l => l.Login)
                .WithOne(login => login.Librarian)
                .HasForeignKey<Librarian>(l => l.LoginId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1-to-Many relationship between Book and BorrowRecords
            modelBuilder.Entity<BorrowRecord>()
                .HasOne(br => br.Book)
                .WithMany(b => b.BorrowRecords)
                .HasForeignKey(br => br.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1-to-Many relationship between Student and BorrowRecords
            modelBuilder.Entity<BorrowRecord>()
                .HasOne(br => br.Student)
                .WithMany(s => s.BorrowRecords)
                .HasForeignKey(br => br.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed Data
            modelBuilder.Entity<Login>().HasData(
                new Login { Id = 1, Username = "admin", PasswordHash = "admin123", Role = "Librarian" },
                new Login { Id = 2, Username = "student1", PasswordHash = "pass123", Role = "Student" }
            );

            modelBuilder.Entity<Librarian>().HasData(
                new Librarian { Id = 1, Name = "Admin Librarian", Email = "admin@library.com", EmployeeId = "LIB-001", Phone = "555-010-9999", Age = 35, LoginId = 1 }
            );

            modelBuilder.Entity<Student>().HasData(
                new Student { Id = 1, Name = "John Doe", Email = "john@example.com", RegistrationNumber = "STU-1001", Address = "123 Main St", Phone = "555-019-2034", LoginId = 2 }
            );

            modelBuilder.Entity<Book>().HasData(
                new Book { Id = 1, Title = "Clean Architecture", Author = "Robert C. Martin", ISBN = "978-0134494166", Publisher = "Prentice Hall", TotalCopies = 5, AvailableCopies = 5 },
                new Book { Id = 2, Title = "Domain-Driven Design", Author = "Eric Evans", ISBN = "978-0321125217", Publisher = "Addison-Wesley", TotalCopies = 3, AvailableCopies = 3 }
            );

            // Global Query Filters (Soft Delete)
            modelBuilder.Entity<Book>().HasQueryFilter(b => !b.IsDeleted);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is not ActivityLog && (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
                .ToList();

            foreach (var entityEntry in entries)
            {
                var actionType = entityEntry.State switch
                {
                    EntityState.Added => "Create",
                    EntityState.Modified => "Edit",
                    EntityState.Deleted => "Delete",
                    _ => "Unknown"
                };

                ActivityLogs.Add(new ActivityLog
                {
                    ActionType = actionType,
                    EntityName = entityEntry.Entity.GetType().Name,
                    Username = "System User",
                    Timestamp = DateTime.Now,
                    Details = $"Entity {actionType} operation executed."
                });
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
