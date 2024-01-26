using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DatabaseConnection.DatabaseAccess;

public class ProjectContext : DbContext
{
    public ProjectContext() { }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=database.db");
        base.OnConfiguring(optionsBuilder);
    }
}
