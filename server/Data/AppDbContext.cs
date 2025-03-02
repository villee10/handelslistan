
using Microsoft.EntityFrameworkCore;
using server.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<ShoppingList> ShoppingLists { get; set; }
    public DbSet<ListItem> ListItems { get; set; }
}

