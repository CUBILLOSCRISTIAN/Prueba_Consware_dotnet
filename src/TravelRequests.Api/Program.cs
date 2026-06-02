using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Register EF Core (requires DefaultConnection in configuration)
builder.Services.AddDBServer(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dependency injection for application services and repositories
builder.Services.AddScoped<TravelRequests.Domain.Repository.IUserRepository, TravelRequests.Infrastructure.Repository.UserRepository>();
builder.Services.AddScoped<TravelRequests.Domain.Services.IAuthService, TravelRequests.Application.Services.AuthService>();
builder.Services.AddScoped<TravelRequests.Domain.Repository.IWorkspaceRepository, TravelRequests.Infrastructure.Repository.WorkspaceRepository>();
builder.Services.AddScoped<TravelRequests.Domain.Services.IWorkspaceService, TravelRequests.Application.Services.WorkspaceService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TravelRequests.Infrastructure.ICurrentWorkspaceProvider, TravelRequests.Infrastructure.CurrentWorkspaceProvider>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "please-configure-a-secure-key";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "travelrequests";
var audience = builder.Configuration["Jwt:Audience"] ?? "travelrequests_clients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey))
        };
    });

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TravelRequests API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
