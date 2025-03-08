using Microsoft.EntityFrameworkCore;
using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Infrastructure.Database;

public class SignaliteDbContext : DbContext
{
    public SignaliteDbContext(DbContextOptions<SignaliteDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<User> Users { get; set; }
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
    }
    
}