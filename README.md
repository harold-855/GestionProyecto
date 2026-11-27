# API REST — Gestión de Proyectos y Tareas

API RESTful desarrollada con **.NET 10**, **Clean Architecture**, **Entity Framework Core**, **PostgreSQL** y **Swagger/OpenAPI** para administrar proyectos, sus tareas asociadas, control de estados, vencimientos y cálculo de avance.

---

## Tabla de Contenido

- [Características Principales](#características-principales)
- [Arquitectura y Estructura](#arquitectura-y-estructura)
- [Modelo de Dominio y Reglas de Negocio](#modelo-de-dominio-y-reglas-de-negocio)
- [Endpoints de la API](#endpoints-de-la-api)
- [Manejo Global de Excepciones](#manejo-global-de-excepciones)
- [Pruebas Automatizadas](#pruebas-automatizadas)
- [Ejecución Local](#ejecución-local)
- [CI/CD y Despliegue en Azure](#cicd-y-despliegue-en-azure)

---

## Características Principales

- **CRUD completo** de Proyectos y Tareas con relación **1:N** y rutas versionadas (`/api/v1/...`) en minúsculas mediante [`LowercaseParameterTransformer`](src/GestionProyecto.Api/Routing/LowercaseParameterTransformer.cs).
- **Serialización legible de enums** (`EstadoTarea` y `PrioridadTarea`) como texto en JSON (`JsonStringEnumConverter`).
- **Consulta de tareas por proyecto** (`GET /api/v1/proyectos/{id}/tareas`).
- **Filtrado flexible de tareas** por estado, prioridad o ambos criterios combinados (`?estado=...&prioridad=...`).
- **Detección de tareas vencidas** no completadas (`GET /api/v1/tareas/vencidas`).
- **Cambio de estado dedicado** mediante `PATCH /api/v1/tareas/{id}/estado`.
- **Cálculo automático de avance del proyecto** basado en tareas completadas (`PorcentajeAvance`, `TotalTareas`, `TareasCompletadas`).
- **Protección de integridad:** impide eliminar proyectos que tengan tareas pendientes o en progreso (`409 Conflict`).
- **Validaciones y errores estandarizados** (`DataAnnotations` + reglas de dominio + middleware global con `ProblemDetails` RFC 7807).
- **Documentación interactiva** con **Swagger UI** habilitada tanto en desarrollo como en producción.

---

## Arquitectura y Estructura

La solución sigue una separación limpia por capas en [`GestionProyecto.slnx`](GestionProyecto.slnx):

```text
GestionProyecto/
├── .github/workflows/
│   └── ci-cd.yml                          # Pipeline de CI/CD en GitHub Actions
├── src/
│   ├── GestionProyecto.Domain/            # Entidades, enums y reglas de negocio puras
│   ├── GestionProyecto.Application/       # DTOs, interfaces, mapeos y casos de uso
│   ├── GestionProyecto.Infrastructure/    # DbContext EF Core (PostgreSQL), migraciones y repositorios
│   └── GestionProyecto.Api/               # Controladores, extensiones, ruteo, middleware y Program.cs
└── tests/
    ├── GestionProyecto.Domain.UnitTests/              # Pruebas unitarias de entidades y reglas de dominio
    ├── GestionProyecto.Application.UnitTests/         # Pruebas unitarias de casos de uso y DTOs (xUnit + Moq)
    ├── GestionProyecto.Infrastructure.IntegrationTests/ # Pruebas de DI, DbContext, configuraciones y repositorios
    ├── GestionProyecto.Api.IntegrationTests/          # Pruebas end-to-end HTTP sobre Kestrel + EF InMemory y middleware
    └── GestionProyecto.ArchitectureTests/             # Pruebas de dependencias de capas y convenciones
```

### Detalle de capas

| Capa | Responsabilidad | Componentes clave |
| :--- | :--- | :--- |
| **Domain** | Lógica de negocio e invariantes | `Proyecto`, `Tarea`, `EstadoTarea`, `PrioridadTarea`, `BaseEntity` |
| **Application** | Orquestación de casos de uso y contratos | `UseCases/Proyectos/*`, `UseCases/Tareas/*`, `DTOs/*`, `Interfaces/*`, `Exceptions/*`, `MappingExtensions`, `DependencyInjection` |
| **Infrastructure** | Persistencia relacional | `GestionProyectoDbContext`, `ProyectoConfiguration`, `TareaConfiguration`, `Migrations/*`, `ProyectoRepository`, `TareaRepository`, `UnitOfWork`, `DependencyInjection` |
| **Api** | Exposición HTTP y configuración | `ProyectosController`, `TareasController`, `ApiControllerBase`, `ApiServiceCollectionExtensions`, `LowercaseParameterTransformer`, `ExceptionHandlingMiddleware`, `Program.cs`, Swagger |

---

## Modelo de Dominio y Reglas de Negocio

- **Proyecto** (`Id`, `Nombre`, `Descripcion`, `FechaInicio`, `FechaFinalizacion`):
  - `Nombre` (máx. 150) y `Descripcion` (máx. 500) son obligatorios.
  - `FechaFinalizacion` debe ser posterior a `FechaInicio`.
  - Las fechas se normalizan a **UTC** para garantizar compatibilidad con PostgreSQL (`timestamp with time zone`).
  - No puede eliminarse si posee tareas cuyo estado sea distinto de `Completada` (`Pendiente` o `EnProgreso`).
  - `PorcentajeAvance = (TareasCompletadas / TotalTareas) * 100` redondeado a 2 decimales (`0` si no hay tareas).

- **Tarea** (`Id`, `Titulo`, `Descripcion`, `Estado`, `Prioridad`, `FechaLimite`, `ProyectoId`):
  - Estados permitidos (`EstadoTarea`): `Pendiente`, `EnProgreso`, `Completada` (inicia siempre en `Pendiente`).
  - Prioridades permitidas (`PrioridadTarea`): `Baja`, `Media`, `Alta`.
  - Debe pertenecer a un `Proyecto` existente.
  - Se considera vencida cuando `FechaLimite < DateTime.UtcNow` y `Estado != Completada`.

---

## Endpoints de la API

### Proyectos (`/api/v1/proyectos`)

| Método | Ruta | Descripción | Respuestas |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/proyectos` | Lista todos los proyectos con su avance | `200 OK` |
| `GET` | `/api/v1/proyectos/{id}` | Obtiene un proyecto por su ID | `200 OK`, `404 Not Found` |
| `GET` | `/api/v1/proyectos/{id}/avance` | Obtiene el resumen y porcentaje de avance | `200 OK`, `404 Not Found` |
| `GET` | `/api/v1/proyectos/{id}/tareas` | Lista las tareas pertenecientes al proyecto | `200 OK`, `404 Not Found` |
| `POST` | `/api/v1/proyectos` | Crea un nuevo proyecto | `201 Created`, `400 Bad Request` |
| `PUT` | `/api/v1/proyectos/{id}` | Actualiza un proyecto existente | `204 No Content`, `400 Bad Request`, `404 Not Found` |
| `DELETE` | `/api/v1/proyectos/{id}` | Elimina un proyecto sin tareas pendientes | `204 No Content`, `404 Not Found`, `409 Conflict` |

### Tareas (`/api/v1/tareas`)

| Método | Ruta | Descripción | Respuestas |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/tareas` | Lista tareas (filtros opcionales `?estado=&prioridad=`) | `200 OK`, `400 Bad Request` |
| `GET` | `/api/v1/tareas/{id}` | Obtiene una tarea por su ID | `200 OK`, `404 Not Found` |
| `GET` | `/api/v1/tareas/estado/{estado}` | Filtra tareas por estado | `200 OK`, `400 Bad Request` |
| `GET` | `/api/v1/tareas/prioridad/{prioridad}` | Filtra tareas por prioridad | `200 OK`, `400 Bad Request` |
| `GET` | `/api/v1/tareas/vencidas` | Consulta tareas vencidas no completadas | `200 OK` |
| `POST` | `/api/v1/tareas` | Crea una tarea asociada a un proyecto | `201 Created`, `400 Bad Request`, `404 Not Found` |
| `PUT` | `/api/v1/tareas/{id}` | Actualiza título, descripción, prioridad y fecha límite | `204 No Content`, `400 Bad Request`, `404 Not Found` |
| `PATCH` | `/api/v1/tareas/{id}/estado` | Cambia el estado de una tarea | `204 No Content`, `400 Bad Request`, `404 Not Found` |
| `DELETE` | `/api/v1/tareas/{id}` | Elimina una tarea | `204 No Content`, `404 Not Found` |

### Documentación y Monitoreo

| Método | Ruta | Descripción |
| :--- | :--- | :--- |
| `GET` | `/swagger` | Interfaz gráfica de Swagger UI |
| `GET` | `/swagger/v1/swagger.json` | Especificación OpenAPI v1 |
| `GET` | `/health` | Verificación de estado de salud de la API |

---

## Manejo Global de Excepciones

El middleware [`ExceptionHandlingMiddleware`](src/GestionProyecto.Api/Middleware/ExceptionHandlingMiddleware.cs) intercepta cualquier excepción no capturada y responde en formato `application/problem+json` (`ProblemDetails`):

| Excepción | Código HTTP | Significado |
| :--- | :--- | :--- |
| `NotFoundException` | `404 Not Found` | El proyecto o tarea solicitada no existe |
| `BusinessRuleException` | `409 Conflict` | Violación de regla de negocio (ej. eliminar proyecto con tareas pendientes) |
| `ArgumentException` / `InvalidOperationException` | `400 Bad Request` | Datos inválidos de dominio o parámetros incorrectos |
| `Exception` (no controlada) | `500 Internal Server Error` | Error inesperado (sin exponer detalles sensibles en producción) |

---

## Pruebas Automatizadas

La solución cuenta con **79 pruebas automatizadas** organizadas en **5 proyectos especializados** bajo `tests/`:

- **`GestionProyecto.Domain.UnitTests`** (**23 pruebas**):
  - `Entities/ProyectoTests.cs`: Validaciones, normalización UTC, gestión de colección de tareas, cálculo de avance y verificación de tareas pendientes.
  - `Entities/TareaTests.cs`: Validaciones, cambios de estado, actualización y lógica de vencimiento.
- **`GestionProyecto.Application.UnitTests`** (**26 pruebas**, `xUnit` + `Moq`):
  - `UseCases/Proyectos/ProyectoUseCasesTests.cs`: Casos de uso de proyectos con repositorios mockeados.
  - `UseCases/Tareas/TareaCrudUseCasesTests.cs`: Casos de uso de creación, actualización, cambio de estado y eliminación de tareas.
  - `UseCases/Tareas/TareaConsultasUseCasesTests.cs`: Casos de uso de consulta, filtrado y tareas vencidas.
  - `DTOs/DtoValidationTests.cs`: Validación de `DataAnnotations` en DTOs de creación de proyectos y tareas.
- **`GestionProyecto.Infrastructure.IntegrationTests`** (**9 pruebas**, `xUnit` + `EF Core InMemory`):
  - `Extensions/DependencyInjectionTests.cs`: Registro de `DbContext`, repositorios y `UnitOfWork`, además de validación de `DefaultConnection`.
  - `Fixtures/DbContextTestFactory.cs`: Fábrica de contexto en memoria para pruebas de persistencia.
  - `Persistence/ModelConfigurationTests.cs`: Verificación de configuraciones Fluent API (`ProyectoConfiguration`, `TareaConfiguration`).
  - `Repositories/ProyectoRepositoryTests.cs`, `Repositories/TareaRepositoryTests.cs`, `Repositories/UnitOfWorkTests.cs`: Persistencia, filtros y `SaveChangesAsync`.
- **`GestionProyecto.Api.IntegrationTests`** (**9 pruebas**, `xUnit` + Kestrel real + `EF Core InMemory`):
  - `Fixtures/ApiTestFactory.cs`: Host Kestrel en puerto efímero con `EF Core InMemory` para pruebas end-to-end.
  - `Endpoints/SwaggerAndHealthTests.cs`: Verificación de `/health`, `/swagger/v1/swagger.json` y rutas `/api/v1/...` en minúsculas.
  - `Endpoints/ProyectosEndpointsTests.cs` y `Endpoints/TareasEndpointsTests.cs`: Flujos end-to-end CRUD, avance, filtros y conflicto `409`.
  - `Middleware/ErrorHandlingTests.cs`: Validaciones y respuestas `ProblemDetails` RFC 7807 en endpoints.
  - `Middleware/ExceptionHandlingMiddlewareTests.cs`: Respuestas `500` de `ExceptionHandlingMiddleware` diferenciando entornos `Development` y `Production`.
- **`GestionProyecto.ArchitectureTests`** (**12 pruebas**):
  - `Helpers/ArchitectureAssemblies.cs`: Utilidades de inspección de ensamblados y referencias.
  - `Layers/LayerDependencyTests.cs`: Reglas de dependencia entre Domain, Application, Infrastructure y Api.
  - `Conventions/DomainAndApplicationConventionsTests.cs` y `Conventions/ApiConventionsTests.cs`: Convenciones de entidades, casos de uso, repositorios, controladores y rutas versionadas en minúsculas.

Ejecutar toda la suite:

```bash
dotnet test GestionProyecto.slnx --configuration Release --verbosity normal
```

---

## Ejecución Local

### Requisitos previos
- [.NET 10 SDK](https://dotnet.microsoft.com/)
- PostgreSQL 15+ (local o en contenedor)

### Pasos

1. **Configurar la cadena de conexión** en `src/GestionProyecto.Api/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=GestionProyectoDb;Username=postgres;Password=postgres"
   }
   ```
2. **Aplicar las migraciones de Entity Framework Core:**
   ```bash
   dotnet ef database update --project src/GestionProyecto.Infrastructure --startup-project src/GestionProyecto.Api
   ```
3. **Compilar y ejecutar la API:**
   ```bash
   dotnet build GestionProyecto.slnx
   dotnet run --project src/GestionProyecto.Api
   ```
4. **Explorar la documentación Swagger** en `http://localhost:5157/swagger` (o `https://localhost:7293/swagger` con el perfil `https`, o usar el archivo de pruebas HTTP `src/GestionProyecto.Api/GestionProyecto.Api.http`).

---

## CI/CD y Despliegue en Azure

El pipeline definido en [`.github/workflows/ci-cd.yml`](.github/workflows/ci-cd.yml) se ejecuta automáticamente en cada `push` y `pull_request` hacia `main` (y manualmente mediante `workflow_dispatch`):

1. **Job `build-and-test`:** Restaura dependencias de `GestionProyecto.slnx`, compila en configuración `Release`, ejecuta las **79 pruebas** de los 5 proyectos (unitarias, integración y arquitectura) y publica el artefacto de la API (`gestion-proyecto-api`).
2. **Job `deploy`:** En eventos `push` sobre `main`, descarga el artefacto publicado y despliega en **Azure App Service** mediante `azure/webapps-deploy@v3` cuando los secretos están configurados.

### Configuración requerida en GitHub y Azure

1. **Secrets en GitHub** (`Settings -> Secrets and variables -> Actions`):
   - `AZURE_WEBAPP_NAME`: Nombre de la instancia en Azure App Service.
   - `AZURE_WEBAPP_PUBLISH_PROFILE`: XML del perfil de publicación obtenido desde Azure Portal.
2. **Variables de entorno en Azure App Service** (`Environment variables`):
   - `ConnectionStrings__DefaultConnection`: Cadena de conexión hacia **Azure Database for PostgreSQL**.
   - `Database__AutoMigrate`: `true` (aplica migraciones pendientes automáticamente al iniciar la aplicación).
