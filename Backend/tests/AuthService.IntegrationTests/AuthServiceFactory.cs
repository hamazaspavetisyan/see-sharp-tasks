using AuthService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.IntegrationTests;

public class AuthServiceFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:Secret", "test-integration-secret-key-at-least-32-chars!!");
        builder.UseSetting("Jwt:Issuer", "TasksApp.AuthService");
        builder.UseSetting("Jwt:Audience", "TasksApp");
        builder.UseSetting("Jwt:ExpiryHours", "1");

        builder.ConfigureServices(services =>
        {
            // EF Core registers IDbContextOptionsConfiguration<TContext> lambdas that
            // keep the UseMySql call alive even after removing DbContextOptions.
            // Remove ALL descriptors related to AuthDbContext configuration.
            var authDbContextOptionsConfigType = typeof(IDbContextOptionsConfiguration<AuthDbContext>);
            var toRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<AuthDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == authDbContextOptionsConfigType)
                .ToList();
            foreach (var d in toRemove)
                services.Remove(d);

            // Guid evaluated here (once, outside the lambda) so all requests share the same DB.
            var dbName = $"AuthIntegration_{Guid.NewGuid()}";
            services.AddDbContext<AuthDbContext>(opt =>
                opt.UseInMemoryDatabase(dbName));
        });

        builder.UseEnvironment("Development");
    }
}
