# TravelRequests — Documentación (Español)

Resumen
-------
Proyecto scaffoldado como una API RESTful .NET con Clean Architecture (capas Domain, Infrastructure, Application, Api). El propósito es implementar un sistema multi-tenant (Workspaces) para gestión de solicitudes de viaje, con patrones de IA opcionales (simulados si no se integran APIs reales).

Estado actual
------------
- Scaffold de la solución y los 4 proyectos creado (`src/TravelRequests.*`).
- Entidades del dominio básicas añadidas: `Workspace`, `User`, `TravelRequest`.
- `AppDbContext` y proveedor de workspace inicial creado; filtro global EF Core preparado.
- Base para repositorios y archivos `csproj` añadidos.
- Quedan por implementar servicios, controladores, autenticación JWT, migraciones y tests.

Requisitos técnicos
------------------
- .NET 9 (recomendado) — el blueprint usa `net9.0`. Se puede adaptar a .NET 6 si es necesario.
- SQL Server y EF Core (migrations).
- JWT para autenticación; el token debe incluir `UserId`, `workspaceId` (GUID) y `Role`.
- Contraseñas con hashing seguro (BCrypt recomendado).

Estructura del proyecto
-----------------------
- `src/TravelRequests.Domain` — entidades, DTOs, enums, interfaces.
- `src/TravelRequests.Infrastructure` — `AppDbContext`, repositorios, configuración EF.
- `src/TravelRequests.Application` — lógica de negocio, servicios, validaciones.
- `src/TravelRequests.Api` — controllers, middleware, `Program.cs`.

Convenciones importantes
-----------------------
- Tablas en `snake_case` mediante Fluent API en `OnModelCreating`.
- Respuesta estándar: `ResponsePackage<T>` con `ErrorResponse`.
- Todos los endpoints deben respetar el `WorkspaceId` del JWT; no aceptar `WorkspaceId` en el body.
- DI: servicios y repositorios `Scoped`.

Patrones de IA (planificados)
----------------------------
Se deben implementar al menos dos. Opciones:
- `TravelRiskService`: evalúa riesgo (Low, Medium, High, Critical) — por defecto se puede simular con heurísticas (duración, fecha, distancia estimada).
- `TravelClassifierService`: clasifica como `NACIONAL`, `INTERNACIONAL`, `REGIONAL`, `URGENTE`, `RECURRENTE` — basado en origen/destino, duración y palabras clave.
- `JustificationReviewService` y `ApprovalPredictorService` son opcionales adicionales.

Comandos útiles (desarrollo local)
---------------------------------
Crear la solución y proyectos (si no se usan los csproj existentes):

```bash
dotnet new sln -n TravelRequests
dotnet new classlib -n TravelRequests.Domain
dotnet new classlib -n TravelRequests.Infrastructure
dotnet new classlib -n TravelRequests.Application
dotnet new webapi -n TravelRequests.Api
dotnet sln add src/TravelRequests.*/*.csproj
```

Instalar paquetes EF Core (en Infrastructure):

```bash
dotnet add src/TravelRequests.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer
dotnet add src/TravelRequests.Infrastructure package Microsoft.EntityFrameworkCore.Design
```

Crear y aplicar migraciones:

```bash
dotnet ef migrations add InitialCreate -p src/TravelRequests.Infrastructure -s src/TravelRequests.Api
dotnet ef database update -p src/TravelRequests.Infrastructure -s src/TravelRequests.Api
```

Cómo ejecutar la API (modo desarrollo):

```bash
cd src/TravelRequests.Api
dotnet run
```

Configuración requerida
----------------------
- `appsettings.json` debe incluir `ConnectionStrings:DefaultConnection` con SQL Server y sección para JWT (secreto, issuer, audience, expiración).
- Variable `workspaceId` en el JWT: usar claim `workspaceId` con valor GUID.

Endpoints (resumen planificado)
------------------------------
- `POST /api/auth/register` — registrar usuario (incluye WorkspaceId para miembros existentes o crea workspace si Owner).
- `POST /api/auth/login` — login y obtención de JWT (payload: `UserId`, `workspaceId`, `role`).
- `POST /api/workspaces` — crear workspace (Owner) — `Owner` será creador.
- `GET /api/workspaces/{id}/members` — listar miembros (Owner only).
- `POST /api/workspaces/{id}/members` — agregar miembro.
- `DELETE /api/workspaces/{id}/members/{userId}` — eliminar miembro.
- `POST /api/travel-requests` — crear solicitud (devuelve `RiskReport` y `Category` si habilitados).
- `GET /api/travel-requests/{id}` — ver detalle (Approver puede ver `ApprovalPrediction` si implementado).

Buenas prácticas y entregables
----------------------------
- Seguir SOLID y Clean Architecture; no exponer entidades de dominio en responses.
- Usar FluentValidation para validar DTOs.
- Registrar al menos 8 commits y usar Conventional Commits.
- Incluir README final, migraciones EF y (opcional) `Dockerfile`/`docker-compose`.

Próximos pasos recomendados
--------------------------
1. Proveer `DefaultConnection` y JWT secret para poder ejecutar migraciones.
2. Implementar `IAuthService`, `IWorkspaceService`, y `ITravelRequestService` en `Application`.
3. Añadir middleware para extraer `workspaceId` del JWT e implementar `ICurrentWorkspaceProvider`.
4. Implementar controladores y pruebas unitarias.

Contacto
--------
Si quieres, puedo continuar implementando los servicios y controladores ahora; dime si prefieres IA simulada o integración real con OpenAI/Azure.
