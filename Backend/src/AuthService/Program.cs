using System.Text;
using AuthService.Application.Commands;
using AuthService.Application.Services;
using AuthService.Infrastructure.Grpc;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Services;
using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Kestrel: HTTP/1+2 on 5100 (REST), HTTP/2 only on 5101 (gRPC)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5100, o => o.Protocols = HttpProtocols.Http1AndHttp2);
    options.ListenLocalhost(5101, o => o.Protocols = HttpProtocols.Http2);
});

// EF Core + MySQL (AutoDetect is not used to avoid startup MySQL connection)
builder.Services.AddDbContext<AuthDbContext>(options =>
{
    var connStr = builder.Configuration.GetConnectionString("AuthDb")!;
    options.UseMySql(connStr, new MySqlServerVersion(new Version(8, 0, 0)));
});

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<RegisterCommand>());

// JWT
var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrEmpty(jwtSecret))
{
    using var lf = LoggerFactory.Create(b => b.AddConsole());
    lf.CreateLogger("Startup").LogCritical(
        "Jwt:Secret is not configured. Set it via user-secrets or an environment variable.");
    throw new InvalidOperationException("Jwt:Secret must be configured via user-secrets or environment.");
}
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TasksApp.AuthService";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "TasksApp";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });
builder.Services.AddAuthorization();

// Services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddHttpContextAccessor();

// FastEndpoints
builder.Services.AddFastEndpoints();

// gRPC
builder.Services.AddGrpc();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints();
app.MapGrpcService<UserValidationGrpcService>();

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program { }
