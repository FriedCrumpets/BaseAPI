using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RESTful_web_API_Course.Core; 

public sealed class ApplicationDbContext(DbContextOptions options) : IdentityDbContext(options) {
    // Create DbSets here
    // public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder builder) {
        base.OnModelCreating(builder); // <- always call base when using identity
        
        // add calculations to make upon building a model
    }
}