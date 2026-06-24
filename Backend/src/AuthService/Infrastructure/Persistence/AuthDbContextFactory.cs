using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuthService.Infrastructure.Persistence;

public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseMySql(
                "Server=localhost;Port=3306;Database=auth_db;Uid=root;Pwd=design_time_placeholder;",
                new MySqlServerVersion(new Version(8, 0, 0)))
            .Options;
        return new AuthDbContext(options);
    }
}
