
namespace RESTful_web_API_Course.Core; 

public sealed class RepositoryManager : IRepositoryManager {
    private readonly ApplicationDbContext _context;

    public RepositoryManager(ApplicationDbContext context) {
        _context = context;

        // Create Repositories here
        // Users = new AuthRepository(context)
    }
   
    // assign Repositories here 
    // public IAuthRepository Users { get; init; }
    
    public async Task Save() {
        await _context.SaveChangesAsync();
    }
}