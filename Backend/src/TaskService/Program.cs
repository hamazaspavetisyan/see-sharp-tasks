using System.Text;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaskService.Application.Commands.Tasks;
using TaskService.Application.Services;
using TaskService.Infrastructure.GrpcClients;
using TaskService.Infrastructure.Persistence;
using TaskService.Protos;

// Required for gRPC over cleartext HTTP/2 on macOS
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

// Kestrel: HTTP/1+2 on 5200 (REST), HTTP/2 only on 5201 (gRPC)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5200, o => o.Protocols = HttpProtocols.Http1AndHttp2);
    options.ListenLocalhost(5201, o => o.Protocols = HttpProtocols.Http2);
});

// EF Core + MySQL
builder.Services.AddDbContext<TasksDbContext>(options =>
{
    var connStr = builder.Configuration.GetConnectionString("TasksDb");
    if (string.IsNullOrEmpty(connStr))
    {
        using var lf = LoggerFactory.Create(b => b.AddConsole());
        lf.CreateLogger("Startup").LogCritical(
            "ConnectionStrings:TasksDb is not configured. Set it via an environment variable.");
        throw new InvalidOperationException("ConnectionStrings:TasksDb must be configured via user-secrets or environment.");
    }
    options.UseMySql(connStr, new MySqlServerVersion(new Version(8, 0, 0)));
});

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<CreateTaskCommand>());

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

// gRPC client → AuthService
var authGrpcAddress = builder.Configuration["AuthService:GrpcAddress"] ?? "http://localhost:5101";
builder.Services.AddGrpcClient<UserValidation.UserValidationClient>(o =>
    o.Address = new Uri(authGrpcAddress));
builder.Services.AddScoped<IUserValidationService, UserValidationClient>();

// FastEndpoints
builder.Services.AddFastEndpoints();
builder.Services.AddHttpContextAccessor();

// Swagger
builder.Services.SwaggerDocument(o =>
{
    o.EnableJWTBearerAuth = true;
    o.DocumentSettings = s =>
    {
        s.Title = "Task Service API";
        s.Version = "v1";
    };
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();
app.UseSwaggerGen();

app.Run();

public partial class Program { }
