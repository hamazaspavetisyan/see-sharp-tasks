using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TaskService.Application.Services;
using TaskService.Infrastructure.Persistence;

namespace TaskService.IntegrationTests;

public class TaskServiceFactory : WebApplicationFactory<Program>
{
    public Mock<IUserValidationService> UserValidationMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:Secret", "test-integration-secret-key-at-least-32-chars!!");
        builder.UseSetting("Jwt:Issuer", "TasksApp.AuthService");
        builder.UseSetting("Jwt:Audience", "TasksApp");

        builder.ConfigureServices(services =>
        {
            // Remove MySQL DbContext registrations (same pattern as AuthService factory)
            var optionsConfigType = typeof(IDbContextOptionsConfiguration<TasksDbContext>);
            var toRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<TasksDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == optionsConfigType)
                .ToList();
            foreach (var d in toRemove)
                services.Remove(d);

            var dbName = $"TasksIntegration_{Guid.NewGuid()}";
            services.AddDbContext<TasksDbContext>(opt => opt.UseInMemoryDatabase(dbName));

            // Replace gRPC UserValidation with mock (AuthService not running in tests)
            var grpcDescriptors = services
                .Where(d => d.ServiceType == typeof(IUserValidationService))
                .ToList();
            foreach (var d in grpcDescriptors)
                services.Remove(d);

            UserValidationMock.Setup(m => m.ValidateUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            services.AddSingleton(UserValidationMock.Object);
        });

        builder.UseEnvironment("Development");
    }
}
