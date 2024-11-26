using Microsoft.EntityFrameworkCore;
using WebApplication13.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<LogEntry> LogEntries { get; set; }
    public DbSet<Chat> Chats { get; set; }
}
