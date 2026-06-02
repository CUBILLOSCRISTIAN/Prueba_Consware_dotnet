# PRUEBA TECNICA: .NET + GIT

### Sistema de Gestion de Solicitudes de Viaje — Workspaces + Patrones de Diseno con

### IA

Duracion: 4 a 6 horas | Nivel: Mid / Senior

## 1. Objetivo General

Desarrollar una API RESTful en .NET 6 o superior que permita a una empresa gestionar
solicitudes de viajes corporativos. Esta version ampliada introduce arquitectura de espacios de
trabajo (Workspaces / Multi-tenant) y patrones de diseno asistidos por inteligencia artificial,
evaluando criterios de escalabilidad, modularidad, seguridad y uso responsable de
herramientas de IA en el ciclo de desarrollo.

## 2. Arquitectura de Workspaces

El candidato debe implementar un sistema de espacios de trabajo que permita a distintas
empresas o equipos operar de forma aislada dentro de la misma API. Cada workspace tiene su
propio conjunto de usuarios, solicitudes y configuracion.

### 2.1 Modelo de datos del Workspace

La entidad Workspace debe incluir como minimo:

- WorkspaceId: Guid
- Name: string
- OwnerId: Guid (usuario propietario)
- CreatedAt: DateTime
- IsActive: bool
- AIConfig: clase de configuracion de funcionalidades IA habilitadas

### 2.2 Roles dentro del Workspace

- OWNER: acceso completo, puede configurar IA y eliminar el workspace
- ADMIN: puede gestionar miembros y solicitudes
- APPROVER: puede aprobar o rechazar solicitudes de su workspace
- REQUESTER: puede crear y ver sus propias solicitudes

### 2.3 Requisitos de implementacion

1. Agregar WorkspaceId como campo obligatorio en todas las entidades (Usuario,
   Solicitud). Usar filtros globales de EF Core para aislar datos por workspace.
2. Crear WorkspaceService con metodos: CreateWorkspace, GetWorkspaceById,
   AddMember, RemoveMember.

3. Implementar WorkspaceMiddleware o claim extractor que lea el WorkspaceId desde el
   JWT y lo inyecte en el contexto de cada request.
4. Proteger todos los endpoints para que un usuario solo pueda acceder a datos de su
   propio workspace.
5. Crear endpoint de administracion: POST /api/workspaces y GET
   /api/workspaces/{id}/members (accesible solo para OWNER).

## 3. Patrones de Diseno con Inteligencia Artificial

Se deben implementar minimo 2 de los siguientes 4 patrones. El uso de APIs de IA reales es
opcional; se puede simular con logica determinista local siempre que la arquitectura y contratos
de datos sean correctos.

### Patron 1 — AI Travel Risk Assessor

Al crear una solicitud de viaje, el sistema evalua automaticamente el nivel de riesgo del
trayecto.

- Crear TravelRiskService con metodo AssessRisk(TravelRequest request):
  Task<RiskReport>.
- Clase RiskReport: { RiskLevel: enum (Low, Medium, High, Critical), Reasons:
  List<string>, Suggestions: List<string>, Score: decimal }.
- La evaluacion puede basarse en: duracion del viaje (dias), distancia entre ciudades
  simulada, fecha (temporada alta/baja).
- Mostrar el RiskReport en la respuesta del endpoint de creacion y en el detalle de la
  solicitud.
- El Approver puede ver el reporte antes de aprobar o rechazar.

### Patron 2 — Smart Request Classifier

El sistema clasifica automaticamente las solicitudes segun el tipo de viaje.

- Crear TravelClassifierService con metodo Classify(TravelRequest request):
  TravelCategory.
- Enum TravelCategory { NACIONAL, INTERNACIONAL, REGIONAL, URGENTE,
  RECURRENTE }.
- La clasificacion puede basarse en: origen/destino, duracion, palabras clave de la
  justificacion.
- Agregar la categoria como campo en la respuesta de listado y detalle de solicitudes.
- Permitir filtrar el listado de solicitudes por categoria.

### Patron 3 — AI Justification Reviewer

Antes de enviar una solicitud, el sistema evalua la calidad de la justificacion ingresada.

- Crear JustificationReviewService con metodo Review(string justification):
  JustificationReport.
- Clase JustificationReport: { Score: decimal, Issues: List<JustificationIssue>,
  Suggestions: List<string>, IsAcceptable: bool }.
- Clase JustificationIssue: { Type: string (length | clarity | relevance), Message: string,
  Severity: string (low | medium | high) }.
- Criterios minimos: longitud minima de 30 caracteres, ausencia de palabras genericas sin
  contexto, coherencia con el destino.

- Retornar el reporte en la respuesta del endpoint de creacion. Si IsAcceptable es false,
  advertir al cliente pero permitir el envio.

### Patron 4 — Predictive Approval Suggester

El sistema sugiere al Approver si debe aprobar o rechazar una solicitud, basandose en el
historial del solicitante.

- Crear ApprovalPredictorService con metodo Predict(Guid userId, TravelRequest
  request): ApprovalPrediction.
- Clase ApprovalPrediction: { SuggestedAction: enum (Approve, Reject, ReviewManually),
  Confidence: decimal, Reasoning: List<string> }.
- La prediccion puede basarse en: cantidad de solicitudes aprobadas/rechazadas previas
  del usuario, frecuencia de viajes, patrones de destino.
- Mostrar la prediccion como campo informativo en el endpoint GET /api/travel-
  requests/{id} cuando el rol es Approver.
- La sugerencia no reemplaza la decision humana; es solo orientativa.

## 4. Requerimientos Funcionales

### 4.1 Gestion de Usuarios

- Registro con campos: Nombre, Correo, Contrasena (hasheada), Rol (Requester,
  Approver, Admin, Owner) y WorkspaceId.
- Inicio de sesion mediante JWT que incluya: UserId, WorkspaceId y Rol en el payload.
- Recuperacion de contrasena: generar codigo unico con expiracion de 5 minutos,
  retornarlo en la respuesta (simulando envio por correo).
- Validacion en el cambio de contrasena: codigo valido, no expirado y correo coincidente.
- Las contrasenas deben almacenarse siempre encriptadas (BCrypt u equivalente).
- Listado de usuarios del workspace (visible solo para roles Admin y Owner).

### 4.2 Solicitudes de Viaje

- Crear solicitud con: Ciudad Origen, Ciudad Destino, Fecha de ida, Fecha de regreso,
  Justificacion.
- Estado inicial: Pendiente. Transiciones permitidas: Pendiente -> Aprobada o Rechazada
  (solo por Approver del workspace).
- Validaciones: fecha regreso > fecha ida, origen distinto al destino, justificacion
  requerida.
- Listar solicitudes propias (Requester) o todas las del workspace (Approver/Admin).
- Integracion con patrones IA: incluir RiskReport, categoria y/o ApprovalPrediction en la
  respuesta segun los patrones implementados.

### 4.3 Gestion de Workspace

- POST /api/workspaces — crear un nuevo workspace (genera WorkspaceId y asigna al
  creador como Owner).
- GET /api/workspaces/{id}/members — listar miembros del workspace.
- POST /api/workspaces/{id}/members — agregar miembro con rol asignado.
- DELETE /api/workspaces/{id}/members/{userId} — remover miembro.

## 5. Consideraciones Tecnicas

### 5.1 Stack requerido

- .NET 6 o superior.
- Clean Architecture: capas API, Application, Domain e Infrastructure.
- SQL Server con Entity Framework Core y migraciones configuradas.
- JWT para autenticacion, con WorkspaceId y Rol en el payload.
- Inyeccion de dependencias nativa de .NET.
- Repository Pattern o DbContext encapsulado.
- Documentacion con Swagger / OpenAPI.
- Validaciones con FluentValidation u otra libreria equivalente.
- Middleware para manejo centralizado de errores.
- Logger configurado (ILogger, Serilog u otro).
- DTOs para entrada y salida de datos en todos los endpoints.

### 5.2 Aislamiento multi-tenant

- Usar filtros globales de EF Core (HasQueryFilter) para filtrar automaticamente por
  WorkspaceId en todas las consultas.
- El WorkspaceId debe extraerse del JWT en cada request y no debe ser enviado por el
  cliente en el body.
- Ningun endpoint debe retornar datos de un workspace distinto al del usuario
  autenticado.

### 5.3 Estructura esperada del proyecto

- /src
  ◦ TravelRequests.Api — Controllers, Middleware, Program.cs
  ◦ TravelRequests.Application — Services, DTOs, Interfaces, Validators, AI Services
  ◦ TravelRequests.Domain — Entities (Workspace, User, TravelRequest), Enums,
  Value Objects
  ◦ TravelRequests.Infrastructure — EF Core, Repositories, Migrations, External
  services
- /tests
  ◦ TravelRequests.Tests — Unit tests de servicios y logica de negocio

### 5.4 Buenas practicas obligatorias

- SOLID: cada servicio tiene una unica responsabilidad.
- Separar logica de negocio en la capa Application; los Controllers solo orquestan.
- No exponer entidades de dominio directamente; usar siempre DTOs.
- Manejo de errores con Result Pattern o excepciones tipadas.
- Todos los metodos async deben retornar Task o Task<T>.

## 6. Seguridad

- Autenticacion con JWT: incluir UserId, WorkspaceId y Rol en el payload.
- Proteccion de rutas segun rol mediante [Authorize(Roles = "...")].

- Almacenamiento de contrasenas con BCrypt u otro algoritmo de hashing seguro.
- Flujo de recuperacion de contrasena con codigo de expiracion de 5 minutos.
- Validar siempre que el WorkspaceId del token coincida con el recurso solicitado.

## 7. Manejo de Git

### 7.1 Estrategia de ramas

- main: solo codigo listo para produccion.
- develop: integracion continua de features.
- feature/workspaces: implementacion del sistema multi-tenant.
- feature/auth: autenticacion JWT y recuperacion de contrasena.
- feature/travel-requests: CRUD de solicitudes.
- feature/ai-[patron]: una rama por patron de IA implementado.
- fix/[descripcion]: correcciones puntuales.

### 7.2 Conventional Commits

Se requiere el uso de Conventional Commits:

- feat(workspace): add multi-tenant isolation with EF Core global filters
- feat(ai): implement TravelRiskService with score-based assessment
- fix(auth): resolve JWT expiration edge case on password recovery
- refactor(application): extract approval logic to dedicated service

Tipos permitidos: feat, fix, refactor, style, test, docs, chore.

### 7.3 Entrega

- Repositorio publico en GitHub.
- README.md con: instrucciones de instalacion y ejecucion, configuracion de base de
  datos, workspaces de prueba disponibles, lista de patrones IA implementados y
  decisiones tecnicas.
- Minimo 8 commits con mensajes descriptivos.
- Pull Request de develop a main con descripcion de cambios.

## 8. Criterios de Evaluacion

- Arquitectura Clean Architecture y separacion de capas — 25%
- Arquitectura de Workspaces y aislamiento multi-tenant — 20%
- Patrones IA implementados (minimo 2) — 20%
- Funcionalidades CRUD + seguridad JWT — 15%
- Buenas practicas, validaciones y manejo de errores — 10%
- Git + documentacion — 10%

## 9. Bonus (No Obligatorio)

- Tests unitarios con xUnit o NUnit para servicios y logica de negocio.
- Dockerfile para levantar la API en contenedor.
- Integracion real con API de IA (OpenAI, Azure AI, Gemini).

- Paginacion y filtros avanzados en el listado de solicitudes.
- Notificaciones simuladas (email o webhook) al cambiar el estado de una solicitud.
- Health check endpoint: GET /health.
- Rate limiting por workspace para prevenir abuso de la API.

## 10. Entregables

6. Enlace al repositorio Git con acceso publico o compartido.
7. README.md con descripcion del proyecto, guia de instalacion, workspaces de prueba
   disponibles (credenciales simuladas), lista de patrones IA implementados y decisiones
   de diseno.
8. Script SQL o migraciones de EF Core listas para ejecutar.
9. Pull Request de develop -> main con descripcion de cambios implementados.
10. (Opcional) URL de despliegue o instrucciones para levantar con Docker.

Nota: el uso de herramientas de IA (Copilot, ChatGPT, Claude, etc.) para asistir en el desarrollo
esta permitido. El candidato debe poder explicar y defender cada decision tecnica.

```
Consware — Prueba Tecnica .NET | Workspaces + IA Design Patterns
```
