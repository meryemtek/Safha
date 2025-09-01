using Microsoft.EntityFrameworkCore;
using Entities;

namespace DataAccessLayer.EntityFramework.Context
{
    public class SafhaDbContext : DbContext
    {
        public SafhaDbContext(DbContextOptions<SafhaDbContext> options) : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<UserBookStatus> UserBookStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Book entity configuration
            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Author).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ISBN).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Publisher).HasMaxLength(100);
                entity.Property(e => e.Genre).HasMaxLength(50);
               // entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
                
                // Relationship with User
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Books)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(100); // PasswordHash yerine Password
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.ProfilePicture).HasMaxLength(500);
                entity.Property(e => e.CoverPhoto).HasMaxLength(500);
                entity.Property(e => e.Bio).HasMaxLength(500);
                entity.Property(e => e.FollowerCount).HasDefaultValue(0);
                entity.Property(e => e.FollowingCount).HasDefaultValue(0);
                entity.Property(e => e.TargetBookCount).HasDefaultValue(0);
                entity.Property(e => e.ReadBookCount).HasDefaultValue(0);
                entity.Property(e => e.Role).HasDefaultValue("User"); // Role property'si eklendi
                entity.Property(e => e.IsActive).HasDefaultValue(true); // IsActive property'si eklendi
                
                // Unique constraints
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // Follow entity configuration
            modelBuilder.Entity<Follow>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Follower relationship
                entity.HasOne(e => e.Follower)
                      .WithMany(u => u.Following)
                      .HasForeignKey(e => e.FollowerId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // Following relationship
                entity.HasOne(e => e.Following)
                      .WithMany(u => u.Followers)
                      .HasForeignKey(e => e.FollowingId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // Composite index to prevent duplicate follows
                entity.HasIndex(e => new { e.FollowerId, e.FollowingId }).IsUnique();
            });

            // Quote entity configuration
            modelBuilder.Entity<Quote>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Author).HasMaxLength(100);
                entity.Property(e => e.Source).HasMaxLength(200);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.PageNumber).HasDefaultValue(0);
                
                // Relationship with User
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Quotes)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                // Relationship with Book
                entity.HasOne(e => e.Book)
                      .WithMany(b => b.Quotes)
                      .HasForeignKey(e => e.BookId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Review entity configuration
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.Title).HasMaxLength(500);
                entity.Property(e => e.Rating).IsRequired().HasDefaultValue(5);
                
                // Relationship with User
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Reviews)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                // Relationship with Book
                entity.HasOne(e => e.Book)
                      .WithMany(b => b.Reviews)
                      .HasForeignKey(e => e.BookId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            
            // UserBookStatus entity configuration
            modelBuilder.Entity<UserBookStatus>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.Notes).HasMaxLength(500);
                
                // Relationship with User
                entity.HasOne(e => e.User)
                      .WithMany(u => u.UserBookStatuses)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                // Relationship with Book
                entity.HasOne(e => e.Book)
                      .WithMany(b => b.UserBookStatuses)
                      .HasForeignKey(e => e.BookId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                // Composite index to prevent duplicate book status entries
                entity.HasIndex(e => new { e.UserId, e.BookId }).IsUnique();
            });
        }
    }
}
