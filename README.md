# TravelRequests — Prueba técnica .NET | Workspaces + IA

API REST en .NET 9 para gestionar solicitudes de viaje corporativas con arquitectura multi-tenant, JWT, validaciones, Swagger y patrones de IA simulados.

## Resumen

La solución sigue Clean Architecture con cuatro capas:

- [src/TravelRequests.Domain](src/TravelRequests.Domain) — entidades, enums, DTOs, contratos.
- [src/TravelRequests.Application](src/TravelRequests.Application) — lógica de negocio, validadores y servicios de IA simulada.
- [src/TravelRequests.Infrastructure](src/TravelRequests.Infrastructure) — EF Core, repositorios y migraciones.
- [src/TravelRequests.Api](src/TravelRequests.Api) — controllers, middleware, Swagger y arranque.

La API fue pensada para cumplir la prueba técnica con foco en aislamiento por workspace, seguridad por JWT y un diseño fácil de extender.

## Qué incluye

- Autenticación JWT con claims de `UserId`, `workspaceId` y `Role`.
- Registro y login.
- Recuperación de contraseña con código único y expiración de 5 minutos.
- Creación y consulta de workspaces.
- Gestión de miembros del workspace.
- Creación, listado y detalle de solicitudes de viaje.
- Dos patrones de IA simulados: riesgo y clasificador.
- Swagger con autenticación Bearer.
- Docker Compose con API + SQL Server.
- Validación automática con FluentValidation.
- Manejo centralizado de errores.

## Requisitos

- Docker y Docker Compose.
- .NET 9 SDK.
- `dotnet-ef` si deseas ejecutar migraciones manualmente.

## Cómo ejecutar

### Con Docker

```bash
docker compose up --build -d
```

- API: `http://localhost:5050`
- Swagger: `http://localhost:5050/swagger/index.html`

### En local

```bash
dotnet restore
dotnet build
dotnet ef database update -p src/TravelRequests.Infrastructure -s src/TravelRequests.Api
cd src/TravelRequests.Api
dotnet run
```

## Configuración

Los valores por defecto para pruebas locales están en [src/TravelRequests.Api/appsettings.json](src/TravelRequests.Api/appsettings.json):

- SQL Server: `Server=mssql,1433;Initial Catalog=TravelRequestsDb;User ID=sa;Password=Your_password123;TrustServerCertificate=True;`
- JWT key: configurada en `Jwt:Key`
- Issuer: `travelrequests`
- Audience: `travelrequests_clients`

> Antes de exponer esto fuera del entorno local, cambia la clave JWT y las credenciales del contenedor.

## Endpoints principales

### Auth

- `POST /api/auth/register` — registra usuario y workspace si no se envía `WorkspaceId`.
- `POST /api/auth/login` — devuelve JWT.
- `POST /api/auth/forgot-password` — genera y retorna un código de recuperación.
- `POST /api/auth/reset-password` — valida código y actualiza la contraseña.

### Workspaces

- `POST /api/workspaces` — crea un workspace y asigna el creador como Owner.
- `GET /api/workspaces/{id}/members` — lista miembros. Requiere rol `Owner` o `Admin`.
- `GET /api/workspaces/{id}/users` — alias para listar usuarios del workspace.
- `POST /api/workspaces/{id}/members` — agrega miembro. Requiere `Owner` o `Admin`.
- `DELETE /api/workspaces/{id}/members/{userId}` — remueve miembro. Requiere `Owner` o `Admin`.

### Travel requests

- `POST /api/travelrequests` — crea solicitud con evaluación IA.
- `GET /api/travelrequests/my` — lista solicitudes propias.
- `GET /api/travelrequests/{id}` — detalle de una solicitud.
- `POST /api/travelrequests/{id}/approve` — aprueba una solicitud. Requiere rol `Approver`.
- `POST /api/travelrequests/{id}/reject` — rechaza una solicitud. Requiere rol `Approver`.

## Flujo de recuperación de contraseña

1. El usuario llama a `forgot-password` con su email.
2. La API genera un código único y lo devuelve en la respuesta.
3. El código expira a los 5 minutos.
4. El usuario llama a `reset-password` con email, código y nueva contraseña.
5. La contraseña se vuelve a guardar con hash BCrypt.

Ejemplo:

```bash
curl -X POST http://localhost:5050/api/auth/forgot-password \
  -H "Content-Type: application/json" \
  -d '{"email":"alice@example.com"}'
```

```bash
curl -X POST http://localhost:5050/api/auth/reset-password \
  -H "Content-Type: application/json" \
  -d '{"email":"alice@example.com","code":"ABC12345","newPassword":"NewPass123!"}'
```

## IA simulada

### Risk Assessor

Al crear una solicitud, se calcula un `RiskReport` con nivel, score, razones y sugerencias.

### Classifier

La solicitud también recibe una categoría de viaje para facilitar clasificación y filtrado.

## Decisiones técnicas

### Clean Architecture

Se separó la solución en capas para que los controladores no contengan lógica de negocio y los detalles de infraestructura queden aislados. Esto facilita pruebas, cambios de persistencia y mantenimiento.

### JWT con `workspaceId`

El `workspaceId` viaja dentro del token para que cada request quede contextualizada por tenant. Así evitamos depender del body para aislar datos y reforzamos la seguridad multi-tenant.

### Filtros globales de EF Core

Se usan `HasQueryFilter` para aplicar el aislamiento por workspace a nivel de consulta. Esto reduce errores de acceso cruzado y centraliza la regla en el ORM.

### FluentValidation

Las validaciones se ejecutan antes de entrar al servicio para reducir errores de dominio y devolver respuestas claras al cliente.

### Middleware centralizado de errores

Los errores no controlados se transforman en una respuesta uniforme. Esto mejora el diagnóstico y evita que cada controller implemente su propio manejo de excepciones.

### Swagger con Bearer auth

Se habilitó Bearer en Swagger para probar el flujo completo sin herramientas externas y facilitar la revisión manual de la API.

### Docker Compose

Se eligió Docker para que la prueba sea reproducible. Con un solo comando se levanta SQL Server y la API con la misma configuración usada en verificación.

### Recuperación de contraseña simulada

Se devuelve el código en la respuesta porque la prueba pide simular el envío por correo. Eso permite validar el flujo sin depender de un proveedor externo.

## Estado de cobertura respecto a la prueba

### Implementado

- Registro y login con JWT.
- Multi-tenant por workspace.
- Recuperación de contraseña con expiración.
- Workspaces y miembros.
- Aprobación y rechazo de solicitudes por rol `Approver`.
- Listado de usuarios del workspace.
- Creación de solicitudes con IA de riesgo y clasificación.
- Swagger, validaciones, Docker y manejo centralizado de errores.

### Pendiente o parcial

- Filtros avanzados y paginación en solicitudes.
- Predicción de aprobación y reviewer de justificación como patrones extra.

## Verificación realizada

- Se reconstruyó la imagen Docker.
- Se validó registro, login y creación de travel request.
- Se validó `forgot-password` y `reset-password`.
- Swagger responde en `http://localhost:5050/swagger/index.html`.

## Observaciones

- El código de recuperación se muestra solo para pruebas locales.
- Las contraseñas se almacenan con BCrypt.
- El proyecto está preparado para evolucionar sin romper la separación de capas.
