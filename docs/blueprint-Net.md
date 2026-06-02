# Blueprint: Backend .NET 9 — Clean Architecture (4 capas)

Este documento es una guía de generación para un LLM. Describe exactamente cómo construir un proyecto de backend ASP.NET Core 9 con la arquitectura y tecnologías definidas aquí. Sigue el orden de las secciones de arriba hacia abajo.

---

## 1. Visión General de la Arquitectura

El proyecto está dividido en 4 proyectos dentro de una solución .NET:

```
{ProjectName}.sln
├── {ProjectName}.Domain/         → Contratos, entidades, DTOs, tipos compartidos
├── {ProjectName}.Infrastructure/ → EF Core, repositorios, acceso a datos
├── {ProjectName}.Application/    → Lógica de negocio (servicios)
└── {ProjectName}.Api/            → Controladores ASP.NET Core, startup
```

**Regla de dependencias (nunca invertir):**

```
Api  ──→  Application  ──→  Domain
          Infrastructure  ──→  Domain
Api  ──→  Infrastructure
```

- `Domain` no tiene dependencias externas (ni NuGet ni referencias a otros proyectos).
- `Infrastructure` y `Application` dependen solo de `Domain`.
- `Api` depende de `Application` e `Infrastructure`.

---

## 2. Crear los Proyectos

El humano ya ha creado la solución `.sln`. El agente recibe el nombre base como input (`{ProjectName}`).

```bash
# Crear proyectos
dotnet new classlib -n {ProjectName}.Domain
dotnet new classlib -n {ProjectName}.Infrastructure
dotnet new classlib -n {ProjectName}.Application
dotnet new webapi   -n {ProjectName}.Api

# Agregar todos a la solución
dotnet sln add {ProjectName}.Domain/{ProjectName}.Domain.csproj
dotnet sln add {ProjectName}.Infrastructure/{ProjectName}.Infrastructure.csproj
dotnet sln add {ProjectName}.Application/{ProjectName}.Application.csproj
dotnet sln add {ProjectName}.Api/{ProjectName}.Api.csproj

# Referencias entre proyectos
dotnet add {ProjectName}.Infrastructure/{ProjectName}.Infrastructure.csproj reference {ProjectName}.Domain/{ProjectName}.Domain.csproj
dotnet add {ProjectName}.Application/{ProjectName}.Application.csproj reference {ProjectName}.Domain/{ProjectName}.Domain.csproj
dotnet add {ProjectName}.Api/{ProjectName}.Api.csproj reference {ProjectName}.Application/{ProjectName}.Application.csproj
dotnet add {ProjectName}.Api/{ProjectName}.Api.csproj reference {ProjectName}.Infrastructure/{ProjectName}.Infrastructure.csproj
```

---

## 3. Capa Domain

**Sin dependencias NuGet.** Solo el SDK base de .NET.

**`{ProjectName}.Domain.csproj`:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

### Estructura de carpetas

```
Domain/
├── Entities/         → POCOs mapeados a tablas
├── Dto/              → Clases de entrada y salida
├── Enums/            → Enumeraciones del dominio
├── Repository/       → Interfaces de repositorios + IUnitOfWork
├── Services/         → Interfaces de servicios de negocio
└── Shared/           → ResponsePackage<T> y ErrorResponse
```

### 3.1 Shared — Clases base de respuesta

**`Shared/ResponsePackage.cs`** — Envuelve TODAS las respuestas de servicios:
```csharp
namespace {ProjectName}.Domain.Shared;

public class ResponsePackage<T>
{
    public ResponsePackage() { }

    public ResponsePackage(string message, T result, ErrorResponse errors)
    {
        Message = message;
        Result = result;
        Errors = errors;
    }

    public string Message { get; set; }
    public T Result { get; set; }
    public ErrorResponse Errors { get; set; }
}
```

**`Shared/ErrorResponse.cs`:**
```csharp
namespace {ProjectName}.Domain.Shared;

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public string Details { get; set; }

    public ErrorResponse(int statusCode, string message, string details = null)
    {
        StatusCode = statusCode;
        Message = message;
        Details = details;
    }
}
```

### 3.2 Repository — Interfaz base y Unit of Work

**`Repository/IBaseRepository.cs`** — Todos los repositorios heredan de aquí:
```csharp
namespace {ProjectName}.Domain.Repository;

public interface IBaseRepository<T> where T : class
{
    Task InsertAsync(T entity);
    Task SaveAsync();
    void Update(T entity);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    void MarkAsModified(T entity);
}
```

**`Repository/IUnitOfWork.cs`:**
```csharp
namespace {ProjectName}.Domain.Repository;

public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
```

**Interfaz de repositorio específico (una por entidad):**
```csharp
namespace {ProjectName}.Domain.Repository;

public interface I{Entity}Repository : IBaseRepository<{Entity}>
{
    // Métodos adicionales propios de la entidad
    Task<List<{Entity}>> GetAll{Entity}s();
}
```

### 3.3 Entities — POCOs con data annotations

Todas las entidades usan data annotations de EF Core. Las tablas se nombran en `snake_case`. Ejemplo:

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace {ProjectName}.Domain.Entities;

[Table("entity_name")]
public class EntityName
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string? SomeField { get; set; }
    public DateTime? CreatedAt { get; set; }

    // FK
    public int? RelatedEntityId { get; set; }

    [ForeignKey("RelatedEntityId")]
    public RelatedEntity? RelatedEntity { get; set; }

    // Colecciones de navegación
    public ICollection<OtherEntity> OtherEntities { get; set; } = new List<OtherEntity>();
}
```

**Tabla de relación muchos-a-muchos (entidad puente):**
```csharp
[Table("entity_a_entity_b_map")]
public class EntityAEntityBMap
{
    public int EntityAId { get; set; }
    public int EntityBId { get; set; }

    [ForeignKey("EntityAId")]
    public EntityA? EntityA { get; set; }

    [ForeignKey("EntityBId")]
    public EntityB? EntityB { get; set; }
}
```

### 3.4 Dto — Data Transfer Objects

Separar DTOs de entrada (request) y salida (response):

```csharp
// Entrada (lo que llega desde la API)
public class Create{Entity}Dto
{
    public string Field1 { get; set; }
    public int RelatedId { get; set; }
    public List<int> RelatedIds { get; set; } = new();
}

// Salida (lo que retorna el servicio)
public class {Entity}ResponseDto
{
    public int Id { get; set; }
    public string Field1 { get; set; }
    public DateTime? CreatedAt { get; set; }
    public RelatedDto? Related { get; set; }
}
```

### 3.5 Enums

```csharp
namespace {ProjectName}.Domain.Enums;

public enum SomeType
{
    ValueA = 0,
    ValueB = 1
}
```

### 3.6 Services — Interfaces de servicios

```csharp
namespace {ProjectName}.Domain.Services;

public interface I{Entity}Service
{
    Task<ResponsePackage<{Entity}ResponseDto>> CreateAsync(Create{Entity}Dto dto);
    Task<ResponsePackage<List<{Entity}>>> GetAllAsync();
}
```

**Para integraciones externas (email, SMS, etc.), definir también su interfaz aquí:**
```csharp
public interface IEmailService
{
    Task<ResponsePackage<bool>> SendNotificationAsync(/* parámetros */);
    Task<ResponsePackage<bool>> SendConfirmationAsync(/* parámetros */);
}
```

**Settings POCO para servicios externos (también en Domain/Entities o Domain/Settings):**
```csharp
public class EmailServiceSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string NotificationEmail { get; set; } = string.Empty;
    public string NotificationTemplateId { get; set; } = string.Empty;
    public string ConfirmationTemplateId { get; set; } = string.Empty;
}
```

---

## 4. Capa Infrastructure

**`{ProjectName}.Infrastructure.csproj`:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.2" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="9.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\{ProjectName}.Domain\{ProjectName}.Domain.csproj" />
  </ItemGroup>
</Project>
```

### 4.1 AppDBContext

Mapear todas las entidades. Las relaciones complejas se configuran en `OnModelCreating` con Fluent API:

```csharp
using Microsoft.EntityFrameworkCore;
using {ProjectName}.Domain.Entities;

namespace {ProjectName}.Infrastructure;

public class AppDBContext : DbContext
{
    public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

    public DbSet<EntityA> EntityAs { get; set; }
    public DbSet<EntityB> EntityBs { get; set; }
    public DbSet<EntityAEntityBMap> EntityAEntityBMaps { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EntityA>().ToTable("entity_a", "dbo");
        modelBuilder.Entity<EntityB>().ToTable("entity_b", "dbo");
        modelBuilder.Entity<EntityAEntityBMap>().ToTable("entity_a_entity_b_map", "dbo");

        // Clave compuesta en tabla puente
        modelBuilder.Entity<EntityAEntityBMap>()
            .HasKey(m => new { m.EntityAId, m.EntityBId });

        // Relaciones muchos-a-muchos
        modelBuilder.Entity<EntityAEntityBMap>()
            .HasOne(m => m.EntityA)
            .WithMany(a => a.EntityBMaps)
            .HasForeignKey(m => m.EntityAId);

        modelBuilder.Entity<EntityAEntityBMap>()
            .HasOne(m => m.EntityB)
            .WithMany(b => b.EntityAMaps)
            .HasForeignKey(m => m.EntityBId);
    }
}
```

### 4.2 DBServerConfig — Extension method para registrar EF Core

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace {ProjectName}.Infrastructure;

public static class DBServerConfig
{
    public static IServiceCollection AddDBServer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDBContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")
            ));

        return services;
    }
}
```

### 4.3 UnitOfWork

```csharp
using Microsoft.EntityFrameworkCore.Storage;
using {ProjectName}.Domain.Repository;

namespace {ProjectName}.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDBContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(AppDBContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
```

### 4.4 BaseRepository

```csharp
using Microsoft.EntityFrameworkCore;
using {ProjectName}.Domain.Repository;

namespace {ProjectName}.Infrastructure.Repository;

public class BaseEfRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly AppDBContext _context;
    protected readonly DbSet<T> _entity;

    public BaseEfRepository(AppDBContext context)
    {
        _context = context;
        _entity = _context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync() => await _entity.ToListAsync();

    public async Task<T?> GetByIdAsync(int id) => await _entity.FindAsync(id);

    public async Task InsertAsync(T entity) => await _entity.AddAsync(entity);

    public async Task SaveAsync() => await _context.SaveChangesAsync();

    public void Update(T entity) => _context.Update(entity);

    public void MarkAsModified(T entity) =>
        _context.Entry(entity).State = EntityState.Modified;
}
```

### 4.5 Repositorios concretos

Cada entidad tiene su repositorio. Heredan de `BaseEfRepository<T>` e implementan la interfaz específica del Domain:

```csharp
using Microsoft.EntityFrameworkCore;
using {ProjectName}.Domain.Entities;
using {ProjectName}.Domain.Repository;

namespace {ProjectName}.Infrastructure.Repository;

public class {Entity}Repository : BaseEfRepository<{Entity}>, I{Entity}Repository
{
    public {Entity}Repository(AppDBContext context) : base(context) { }

    public async Task<List<{Entity}>> GetAll{Entity}s()
    {
        return await _entity
            .AsNoTracking()
            .Include(e => e.RelatedEntity)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }
}
```

**Repositorio para entidad puente (sin métodos adicionales):**
```csharp
public class {Entity}MapRepository : BaseEfRepository<{Entity}Map>, I{Entity}MapRepository
{
    public {Entity}MapRepository(AppDBContext context) : base(context) { }
}
```

### 4.6 SqlConnectionFactory y AdoHelper (acceso ADO.NET opcional)

Útil para queries complejos fuera del ORM:

```csharp
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace {ProjectName}.Infrastructure;

public static class SqlConnectionFactory
{
    private static string? _connectionString;

    public static void Initialize(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found.");
    }

    public static SqlConnection CreateConnection()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
            throw new InvalidOperationException("Connection string not initialized.");
        return new SqlConnection(_connectionString);
    }

    public static async Task<SqlConnection> GetOpenConnectionAsync()
    {
        var conn = CreateConnection();
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync();
        return conn;
    }
}
```

```csharp
using System.Data;
using Microsoft.Data.SqlClient;

namespace {ProjectName}.Infrastructure;

public static class AdoHelper
{
    public static async Task<List<T>> ExecuteReaderAsync<T>(
        string sql,
        Func<SqlDataReader, T> map,
        IEnumerable<SqlParameter>? parameters = null)
    {
        var result = new List<T>();
        await using var conn = await SqlConnectionFactory.GetOpenConnectionAsync();
        await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
        if (parameters != null) cmd.Parameters.AddRange(parameters.ToArray());
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync()) result.Add(map(reader));
        return result;
    }

    public static async Task<T?> ExecuteSingleAsync<T>(
        string sql,
        Func<SqlDataReader, T> map,
        IEnumerable<SqlParameter>? parameters = null)
    {
        await using var conn = await SqlConnectionFactory.GetOpenConnectionAsync();
        await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
        if (parameters != null) cmd.Parameters.AddRange(parameters.ToArray());
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync()) return map(reader);
        return default;
    }

    public static async Task<int> ExecuteNonQueryAsync(
        string sql,
        IEnumerable<SqlParameter>? parameters = null)
    {
        await using var conn = await SqlConnectionFactory.GetOpenConnectionAsync();
        await using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };
        if (parameters != null) cmd.Parameters.AddRange(parameters.ToArray());
        return await cmd.ExecuteNonQueryAsync();
    }
}
```

---

## 5. Capa Application

**`{ProjectName}.Application.csproj`:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Options" Version="9.0.0" />
    <!-- Agregar solo los paquetes de integraciones externas que se usen -->
    <!-- <PackageReference Include="SendGrid" Version="9.29.3" /> -->
    <!-- <PackageReference Include="Twilio" Version="7.14.0" /> -->
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\{ProjectName}.Domain\{ProjectName}.Domain.csproj" />
  </ItemGroup>
</Project>
```

### 5.1 Servicio simple (un repositorio)

```csharp
using {ProjectName}.Domain.Entities;
using {ProjectName}.Domain.Repository;
using {ProjectName}.Domain.Services;
using {ProjectName}.Domain.Shared;

namespace {ProjectName}.Application.Services;

public class {Entity}Service : I{Entity}Service
{
    private readonly I{Entity}Repository _{entity}Repository;

    public {Entity}Service(I{Entity}Repository {entity}Repository)
    {
        _{entity}Repository = {entity}Repository;
    }

    public async Task<ResponsePackage<List<{Entity}>>> GetAllAsync()
    {
        var response = new ResponsePackage<List<{Entity}>>();
        try
        {
            var items = await _{entity}Repository.GetAll{Entity}s();
            if (items == null)
            {
                response.Message = "No records found";
                response.Errors = new ErrorResponse(404, "No records found");
                return response;
            }
            response.Message = "OK";
            response.Result = items;
        }
        catch (Exception ex)
        {
            response.Errors = new ErrorResponse(500, ex.Message);
            response.Message = ex.Message;
        }
        return response;
    }
}
```

### 5.2 Servicio complejo (multi-repositorio + transacción)

Patrón para operaciones que escriben en múltiples tablas:

```csharp
public class ComplexEntityService : IComplexEntityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEntityARepository _entityARepository;
    private readonly IEntityBRepository _entityBRepository;
    private readonly IExternalService _externalService;

    public ComplexEntityService(
        IUnitOfWork unitOfWork,
        IEntityARepository entityARepository,
        IEntityBRepository entityBRepository,
        IExternalService externalService)
    {
        _unitOfWork = unitOfWork;
        _entityARepository = entityARepository;
        _entityBRepository = entityBRepository;
        _externalService = externalService;
    }

    public async Task<ResponsePackage<EntityAResponseDto>> CreateAsync(CreateEntityADto dto)
    {
        var response = new ResponsePackage<EntityAResponseDto>();

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // 1. Crear entidad principal
            var entityA = new EntityA { /* mapear desde dto */ CreatedAt = DateTime.UtcNow };
            await _entityARepository.InsertAsync(entityA);
            await _entityARepository.SaveAsync();

            // 2. Crear entidades relacionadas si aplica
            if (dto.SomeCondition)
            {
                var entityB = new EntityB { EntityAId = entityA.Id, /* ... */ };
                await _entityBRepository.InsertAsync(entityB);
                await _entityBRepository.SaveAsync();
            }

            await _unitOfWork.CommitAsync();

            // 3. Operaciones post-commit (notificaciones, no deben fallar el request)
            try
            {
                await _externalService.SendNotificationAsync(/* ... */);
            }
            catch
            {
                // Log, pero no propagar el error
            }

            response.Message = "OK";
            response.Result = new EntityAResponseDto { Id = entityA.Id, /* ... */ };
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            response.Errors = new ErrorResponse(500, ex.Message);
            response.Message = ex.Message;
        }

        return response;
    }
}
```

### 5.3 Servicio de integración externa (email, SMS)

Inyectar la configuración con `IOptions<TSettings>`:

```csharp
using Microsoft.Extensions.Options;
using {ProjectName}.Domain.Entities;
using {ProjectName}.Domain.Services;
using {ProjectName}.Domain.Shared;

namespace {ProjectName}.Application.Services;

public class EmailService : IEmailService
{
    private readonly EmailServiceSettings _settings;

    public EmailService(IOptions<EmailServiceSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<ResponsePackage<bool>> SendNotificationAsync(/* parámetros */)
    {
        var response = new ResponsePackage<bool>();
        try
        {
            // Lógica con el SDK externo usando _settings.ApiKey, etc.
            response.Message = "OK";
            response.Result = true;
        }
        catch (Exception ex)
        {
            response.Errors = new ErrorResponse(500, ex.Message);
            response.Message = ex.Message;
        }
        return response;
    }
}
```

---

## 6. Capa Api

**`{ProjectName}.Api.csproj`:**
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="7.2.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\{ProjectName}.Application\{ProjectName}.Application.csproj" />
    <ProjectReference Include="..\{ProjectName}.Infrastructure\{ProjectName}.Infrastructure.csproj" />
  </ItemGroup>
</Project>
```

### 6.1 Program.cs

Seguir este orden exacto de configuración:

```csharp
using System.Text.Json.Serialization;
using {ProjectName}.Application.Services;
using {ProjectName}.Domain.Entities;
using {ProjectName}.Domain.Repository;
using {ProjectName}.Domain.Services;
using {ProjectName}.Infrastructure;
using {ProjectName}.Infrastructure.Repository;

var builder = WebApplication.CreateBuilder(args);

// 1. Controladores con opciones JSON
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// 2. CORS — orígenes específicos del frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("https://your-frontend.com", "https://www.your-frontend.com")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 3. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 4. Health Checks
builder.Services.AddHealthChecks();

// 5. Base de datos (EF Core via extension method de Infrastructure)
builder.Services.AddDBServer(builder.Configuration);

// 6. Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 7. Servicios de negocio
builder.Services.AddScoped<I{Entity}Service, {Entity}Service>();
// builder.Services.AddScoped<IEmailService, EmailService>();

// 8. Repositorios
builder.Services.AddScoped<I{Entity}Repository, {Entity}Repository>();

// 9. Configuración de servicios externos (Options pattern)
// builder.Services.Configure<EmailServiceSettings>(builder.Configuration.GetSection("EmailService"));

var app = builder.Build();

// Middleware — este orden es importante
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "{ProjectName} API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHealthChecks("/health");
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("FrontendPolicy");
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### 6.2 Controladores

```csharp
using Microsoft.AspNetCore.Mvc;
using {ProjectName}.Domain.Dto;
using {ProjectName}.Domain.Services;

namespace {ProjectName}.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class {Entity}Controller : ControllerBase
{
    private readonly I{Entity}Service _{entity}Service;

    public {Entity}Controller(I{Entity}Service {entity}Service)
    {
        _{entity}Service = {entity}Service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var response = await _{entity}Service.GetAllAsync();
            return response.Errors is null ? Ok(response) : StatusCode(response.Errors.StatusCode, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Create{Entity}Dto dto)
    {
        try
        {
            var response = await _{entity}Service.CreateAsync(dto);
            return response.Errors is null ? Ok(response) : StatusCode(response.Errors.StatusCode, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
```

> **Nota:** Endpoints GET masivos (`GetAll`) que no sean necesarios desde el inicio deben dejarse comentados hasta ser validados y requeridos.

### 6.3 appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:your-server.database.windows.net,1433;Initial Catalog=YOUR-DB;Persist Security Info=False;User ID=your_user;Password=your_password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  },
  "EmailService": {
    "ApiKey": "",
    "FromEmail": "",
    "FromName": "",
    "NotificationEmail": "",
    "NotificationTemplateId": "",
    "ConfirmationTemplateId": ""
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**`appsettings.Development.json`:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

---

## 7. Convenciones

| Aspecto | Convención |
|---|---|
| Tablas en DB | `snake_case` (configurado en `OnModelCreating`, no en data annotations) |
| Ciclo de vida DI | `Scoped` para todos los servicios y repositorios |
| Serialización de enums | Como strings (`JsonStringEnumConverter`) |
| Nullable/ImplicitUsings | Habilitados en los 4 proyectos |
| Target framework | `net9.0` |
| Errores en servicios externos | Se silencian (`try/catch` vacío) para no fallar el request principal |
| Swagger | Solo en `Development` y `Staging` |
| CORS | Orígenes explícitos, nunca wildcard en producción |
| Patrón de respuesta | Siempre `ResponsePackage<T>`; `Errors is null` determina el status code HTTP |
| Transacciones | Solo cuando se escribe en múltiples tablas; usar `IUnitOfWork` |

---

## 8. Patrón para Crear Nuevos Endpoints

Al agregar un nuevo endpoint, seguir siempre esta distribución de responsabilidades:

### Regla principal
- **Los repositorios hacen las consultas** (queries con filtros, includes, ordenamientos).
- **Los servicios usan los métodos de `BaseEfRepository`** para crear, actualizar o eliminar. No escriben queries EF Core directamente en los servicios.

### Dónde va cada cosa

| Responsabilidad | Capa | Ejemplo |
|---|---|---|
| Query con filtro, include, ordenamiento | `Repository/{Entity}Repository.cs` | `GetByIso2Async`, `GetByIdsAsync`, `GetAllWithRelationsAsync` |
| Crear entidad | `Application/{Entity}Service.cs` | `await _repo.InsertAsync(entity)` + `await _repo.SaveAsync()` |
| Actualizar entidad | `Application/{Entity}Service.cs` | `_repo.MarkAsModified(entity)` + `await _repo.SaveAsync()` |
| Lógica de negocio (validaciones, mapeos, condiciones) | `Application/{Entity}Service.cs` | Cualquier `if`, transformación de DTOs a entidades |
| Retornar la respuesta HTTP | `Api/Controllers/{Entity}Controller.cs` | `return response.Errors is null ? Ok(response) : StatusCode(...)` |

### Flujo completo al agregar un endpoint

**1. Definir la interfaz en Domain** (si no existe el método):
```csharp
// Domain/Repository/I{Entity}Repository.cs
Task<List<{Entity}>> GetByFilterAsync(string filter);
```

**2. Implementar la query en el repositorio** (Infrastructure):
```csharp
// Infrastructure/Repository/{Entity}Repository.cs
public async Task<List<{Entity}>> GetByFilterAsync(string filter)
{
    return await _entity
        .AsNoTracking()
        .Where(e => e.SomeField == filter)
        .Include(e => e.Relation)
        .OrderByDescending(e => e.CreatedAt)
        .ToListAsync();
}
```

**3. Definir el método en la interfaz de servicio** (Domain):
```csharp
// Domain/Services/I{Entity}Service.cs
Task<ResponsePackage<List<{Entity}ResponseDto>>> GetByFilterAsync(string filter);
```

**4. Implementar el servicio** usando métodos del repositorio (Application):
```csharp
// Application/Services/{Entity}Service.cs

// Para LECTURA — delegar la query al repositorio:
public async Task<ResponsePackage<List<{Entity}ResponseDto>>> GetByFilterAsync(string filter)
{
    var response = new ResponsePackage<List<{Entity}ResponseDto>>();
    try
    {
        var items = await _{entity}Repository.GetByFilterAsync(filter);
        response.Message = "OK";
        response.Result = items.Select(e => new {Entity}ResponseDto { /* mapeo */ }).ToList();
    }
    catch (Exception ex)
    {
        response.Errors = new ErrorResponse(500, ex.Message);
        response.Message = ex.Message;
    }
    return response;
}

// Para ESCRITURA — usar métodos base (InsertAsync, SaveAsync, MarkAsModified):
public async Task<ResponsePackage<{Entity}ResponseDto>> CreateAsync(Create{Entity}Dto dto)
{
    var response = new ResponsePackage<{Entity}ResponseDto>();
    await _unitOfWork.BeginTransactionAsync(); // solo si involucra múltiples tablas
    try
    {
        var entity = new {Entity}
        {
            Field1 = dto.Field1,
            CreatedAt = DateTime.UtcNow
        };
        await _{entity}Repository.InsertAsync(entity);  // ← método de BaseEfRepository
        await _{entity}Repository.SaveAsync();           // ← método de BaseEfRepository
        await _unitOfWork.CommitAsync();

        response.Message = "OK";
        response.Result = new {Entity}ResponseDto { Id = entity.Id };
    }
    catch (Exception ex)
    {
        await _unitOfWork.RollbackAsync();
        response.Errors = new ErrorResponse(500, ex.Message);
        response.Message = ex.Message;
    }
    return response;
}
```

**5. Agregar el action en el controlador** (Api):
```csharp
// Api/Controllers/{Entity}Controller.cs
[HttpGet("{filter}")]
public async Task<IActionResult> GetByFilter(string filter)
{
    try
    {
        var response = await _{entity}Service.GetByFilterAsync(filter);
        return response.Errors is null ? Ok(response) : StatusCode(response.Errors.StatusCode, response);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = ex.Message });
    }
}
```

### Resumen de métodos disponibles en BaseEfRepository

| Método | Cuándo usarlo |
|---|---|
| `InsertAsync(entity)` | Antes de guardar una nueva entidad |
| `SaveAsync()` | Después de InsertAsync o MarkAsModified |
| `Update(entity)` | Adjunta y rastrea una entidad modificada |
| `MarkAsModified(entity)` | Marca la entidad como `Modified` sin rastrearla antes |
| `GetAllAsync()` | Lista completa sin filtros (usar con cautela en tablas grandes) |
| `GetByIdAsync(id)` | Buscar por clave primaria |

> **Regla:** si necesitas un query con `Where`, `Include`, `OrderBy` o cualquier filtro personalizado → crea el método en el repositorio concreto, no en el servicio.

---

## 9. Orden de Generación Recomendado

Seguir este orden para evitar referencias circulares durante la generación:

1. **Crear proyectos y referencias** (comandos `dotnet new` + `dotnet add reference`)
2. **Domain**
   - `Shared/`: `ResponsePackage<T>`, `ErrorResponse`
   - `Enums/`: enumeraciones del dominio
   - `Entities/`: POCOs (empezar por entidades sin dependencias)
   - `Repository/`: `IBaseRepository<T>`, `IUnitOfWork`, interfaces específicas
   - `Services/`: interfaces de servicio, Settings POCOs de integraciones externas
   - `Dto/`: DTOs de entrada y salida
3. **Infrastructure**
   - `AppDBContext.cs`
   - `DBServerConfig.cs`
   - `UnitOfWork.cs`
   - `SqlConnectionFactory.cs` + `AdoHelper.cs`
   - `Repository/BaseRepository.cs`
   - `Repository/{Entity}Repository.cs` (uno por entidad)
4. **Application**
   - `Services/{Entity}Service.cs` (uno por entidad, primero los simples)
   - `Services/ExternalIntegrationService.cs` (email, SMS, etc.)
5. **Api**
   - `appsettings.json` + `appsettings.Development.json`
   - `Program.cs`
   - `Controllers/{Entity}Controller.cs` (uno por entidad)
