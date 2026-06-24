using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

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
    .AddJwtBearer("Bearer", options =>
    {
        options.MapInboundClaims = false;
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

builder.Services.AddOcelot(builder.Configuration); // xxxx todo, how Addocelot became available in the Services ?

var app = builder.Build();

app.Use(async (ctx, next) =>
{
    var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    ctx.Request.Headers["X-Real-IP"] = ip;
    await next();
});

app.UseAuthentication();
await app.UseOcelot();

app.Run();
