using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SignaliteWebAPI.Domain.Models;
using Serilog;
namespace SignaliteWebAPI.Infrastructure.Database;

public class SignaliteDbContext : DbContext
{
    public SignaliteDbContext(DbContextOptions<SignaliteDbContext> options) : base(options)
    {
        
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // optionsBuilder.LogTo(Log.Information, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information); (redundant)
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<UserFriend> UserFriends { get; set; }
    public DbSet<FriendRequest> FriendRequests { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<UserGroup> UserGroups { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<MessageRead> ReadBy { get; set; }
    public DbSet<Attachment> Attachments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FriendRequest>()
            .HasOne(f => f.Sender)
            .WithMany(f => f.FriendRequests)
            .HasForeignKey(f => f.SenderId);
        
        modelBuilder.Entity<UserFriend>()
            .HasOne(f => f.Friend)
            .WithMany(f => f.Friends)
            .HasForeignKey(f => f.FriendId);
        
        modelBuilder.Entity<Attachment>()
            .HasOne(a => a.Message)
            .WithOne(m => m.Attachment)
            .HasForeignKey<Attachment>(a => a.MessageId);

        modelBuilder.Entity<User>()
            .HasOne(u => u.ProfilePhoto)
            .WithOne()
            .HasForeignKey<User>(u => u.ProfilePhotoId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();
            
            entity.Property(e => e.RefreshTokenExpiryDate)
                .HasColumnType("timestamp with time zone");
        });
        
        modelBuilder.Entity<User>()
            .HasOne(u => u.BackgroundPhoto)
            .WithOne()
            .HasForeignKey<User>(u => u.BackgroundPhotoId)
            .OnDelete(DeleteBehavior.SetNull);
    }
    
}