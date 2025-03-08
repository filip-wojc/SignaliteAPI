using Microsoft.EntityFrameworkCore;
using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Infrastructure.Database;

public class SignaliteDbContext : DbContext
{
    public SignaliteDbContext(DbContextOptions<SignaliteDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<UserGroup> UserGroups { get; set; }
}