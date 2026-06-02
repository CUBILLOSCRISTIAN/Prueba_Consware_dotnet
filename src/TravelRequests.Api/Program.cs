using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using TravelRequests.Api.Middleware;
using TravelRequests.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Register EF Core (requires DefaultConnection in configuration)
builder.Services.AddDBServer(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<TravelRequests.Application.Validators.CreateTravelRequestDtoValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TravelRequests API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by a space and your JWT token."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Dependency injection for application services and repositories
builder.Services.AddScoped<TravelRequests.Domain.Repository.IUserRepository, TravelRequests.Infrastructure.Repository.UserRepository>();
builder.Services.AddScoped<TravelRequests.Domain.Services.IAuthService, TravelRequests.Application.Services.AuthService>();
builder.Services.AddScoped<TravelRequests.Domain.Repository.IWorkspaceRepository, TravelRequests.Infrastructure.Repository.WorkspaceRepository>();
builder.Services.AddScoped<TravelRequests.Domain.Services.IWorkspaceService, TravelRequests.Application.Services.WorkspaceService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TravelRequests.Infrastructure.ICurrentWorkspaceProvider, TravelRequests.Infrastructure.CurrentWorkspaceProvider>();
builder.Services.AddScoped<TravelRequests.Domain.Repository.ITravelRequestRepository, TravelRequests.Infrastructure.Repository.TravelRequestRepository>();
builder.Services.AddScoped<TravelRequests.Domain.Services.ITravelRiskService, TravelRequests.Application.Services.TravelRiskService>();
builder.Services.AddScoped<TravelRequests.Domain.Services.ITravelClassifierService, TravelRequests.Application.Services.TravelClassifierService>();
builder.Services.AddScoped<TravelRequests.Domain.Services.ITravelRequestService, TravelRequests.Application.Services.TravelRequestService>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "please-configure-a-secure-key";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "travelrequests";
var audience = builder.Configuration["Jwt:Audience"] ?? "travelrequests_clients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
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

builder.Services.AddAuthorization();

var app = builder.Build();

// Apply EF Core migrations automatically at startup (if DB reachable)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetService<TravelRequests.Infrastructure.AppDbContext>();
        if (db != null)
        {
            db.Database.Migrate();
        }
    }
    catch
    {
        // swallow startup migration errors; useful when DB not yet ready
    }
}

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TravelRequests API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
