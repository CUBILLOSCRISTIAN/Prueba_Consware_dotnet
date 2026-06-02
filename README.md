# TravelRequests — API REST .NET (Workspaces + IA)

Este repositorio contiene un scaffold funcional de una API RESTful en .NET 9 con
arquitectura Clean (Domain / Application / Infrastructure / Api) para gestionar
solicitudes de viaje multi-tenant (Workspaces) e incluir patrones IA simulados.

Contenido principal

- Código fuente: [src/](src/)
- Configuración API: [src/TravelRequests.Api/appsettings.json](src/TravelRequests.Api/appsettings.json)
- Docker Compose: [docker-compose.yml](docker-compose.yml)
- Documentación en español: [DOCUMENTACION_ES.md](DOCUMENTACION_ES.md)

Requisitos

- Docker & Docker Compose (para ejecutar base de datos y API en contenedores)
- .NET 9 SDK (si desea compilar/ejecutar localmente sin Docker)
- `dotnet-ef` (para generar/aplicar migraciones localmente): `dotnet tool install --global dotnet-ef`

Estructura de proyectos

- `src/TravelRequests.Domain` — Entidades, DTOs, interfaces.
- `src/TravelRequests.Infrastructure` — `AppDbContext`, repositorios, EF Core.
- `src/TravelRequests.Application` — Servicios de negocio (Auth, Workspace, TravelRequest, IA simulada).
- `src/TravelRequests.Api` — Controladores, `Program.cs`, `appsettings.json`, Dockerfile.

Configuración importante

- Revisa y ajusta la cadena de conexión en [src/TravelRequests.Api/appsettings.json](src/TravelRequests.Api/appsettings.json).
  Por defecto el `docker-compose.yml` provee una DB SQL Server con:
  - Usuario: `sa`
  - Password: `Your_password123`

- La clave JWT por defecto está en `Jwt:Key` dentro de [src/TravelRequests.Api/appsettings.json](src/TravelRequests.Api/appsettings.json).
  Cámbiala por una clave segura antes de exponer la API.

Ejecutar localmente (sin Docker)

1. Restaurar y compilar:

```bash
dotnet restore
dotnet build
```

2. Crear migraciones y aplicar la DB (si usas una instancia SQL Server local):

```bash
dotnet ef migrations add InitialCreate -p src/TravelRequests.Infrastructure -s src/TravelRequests.Api
dotnet ef database update -p src/TravelRequests.Infrastructure -s src/TravelRequests.Api
```

3. Ejecutar la API:

```bash
cd src/TravelRequests.Api
dotnet run
```

Ejecutar con Docker Compose (recomendado para pruebas rápidas)

1. Construir y levantar todos los servicios (SQL Server + API):

```bash
docker compose up --build
```

2. Por defecto la API queda expuesta en `http://localhost:5000` (mapea al puerto 80 del contenedor).

Cambiar puerto

- Si quieres exponer la API en otro puerto, edita `docker-compose.yml` (sección `api.ports`) o pasa una variable de entorno.

Migraciones automáticas

- El `Program.cs` intenta ejecutar `db.Database.Migrate()` al iniciar la API. Si el contenedor de SQL Server tarda en estar listo, puede que la migración falle silenciosamente; en ese caso aplica las migraciones manualmente usando `dotnet ef` (comandos arriba) o ejecuta `docker compose up` y luego `docker compose exec api dotnet ef database update -p src/TravelRequests.Infrastructure -s src/TravelRequests.Api`.

Endpoints principales (resumen)

- `POST /api/auth/register` — Registrar usuario. Payload: `{ "name","email","password","workspaceId","role" }`.
- `POST /api/auth/login` — Login. Payload: `{ "email","password" }`. Respuesta contiene `token` JWT.
- `POST /api/workspaces` — Crear workspace (autenticado).
- `GET /api/workspaces/{id}/members` — Listar miembros (Owner only).
- `POST /api/travelrequests` — Crear solicitud de viaje (autenticado). Devuelve `RiskReport` y `Category` si habilitados.
- `GET /api/travelrequests/my` — Listar solicitudes propias.
- `GET /api/travelrequests/{id}` — Detalle de solicitud.

Uso rápido con curl (ejemplo)

1. Registrar usuario (sin workspace existente — para pruebas puede usar un `workspaceId` nuevo):

```bash
curl -X POST http://localhost:5000/api/auth/register \
	-H "Content-Type: application/json" \
	-d '{"name":"Alice","email":"alice@example.com","password":"Pass123!","workspaceId":"00000000-0000-0000-0000-000000000000","role":"Owner"}'
```

2. Login y usar token:

```bash
curl -X POST http://localhost:5000/api/auth/login \
	-H "Content-Type: application/json" \
	-d '{"email":"alice@example.com","password":"Pass123!"}'

# Usar el token devuelto en Authorization: Bearer <token>
curl -H "Authorization: Bearer <token>" http://localhost:5000/api/travelrequests/my
```

Notas de seguridad y diseño

- No expongas `Jwt:Key` ni contraseñas en repositorios públicos.
- El `WorkspaceId` se extrae del JWT y se utiliza para aplicar filtros globales en EF Core; no envíes `WorkspaceId` en bodies de las requests salvo donde la API explícitamente lo requiera.

Pruebas y siguientes pasos

- Añadir pruebas unitarias (xUnit) en `tests/TravelRequests.Tests`.
- Limpiar warnings relacionados con métodos `GetByIdAsync` duplicados en interfaces (no crítico).
- Implementar integración real con APIs de IA si se desea (actualmente los servicios IA están simulados).

Contribuciones

- Usa la estrategia de ramas propuesta en la especificación y mensajes de commits con Conventional Commits.

Contacto

- Si quieres, puedo generar ejemplos de Postman collection, agregar tests unitarios básicos o preparar el PR final. ¿Qué prefieres que haga a continuación?
