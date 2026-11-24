# Canchero API

API REST para consultar canchas deportivas, administrar sedes y gestionar reservas. Está construida con .NET 10, ASP.NET Core, Entity Framework Core, PostgreSQL y ASP.NET Core Identity.

## Contenido

- [Requisitos](#requisitos)
- [Inicio rápido](#inicio-rápido)
- [Ejecutar desde Rider](#ejecutar-desde-rider)
- [Configuración](#configuración)
- [Base de datos, migraciones y datos iniciales](#base-de-datos-migraciones-y-datos-iniciales)
- [Autenticación y permisos](#autenticación-y-permisos)
- [API HTTP](#api-http)
- [Ejemplos de uso](#ejemplos-de-uso)
- [Modelo y reglas de negocio](#modelo-y-reglas-de-negocio)
- [Estructura de la solución](#estructura-de-la-solución)
- [Solución de problemas](#solución-de-problemas)

## Requisitos

- .NET SDK 10.
- PostgreSQL disponible localmente o en un servidor accesible.
- Base de datos PostgreSQL creada. La migración inicial crea las tablas, pero no crea la base de datos del servidor.
- Para buscar por nombre de lugar se requiere acceso a Internet hacia Nominatim/OpenStreetMap.

Comprueba el SDK instalado:

```bash
dotnet --version
```

## Inicio rápido

Desde la raíz del repositorio:

```bash
dotnet restore canchero.slnx
dotnet run --project src/Presentation/Presentation.csproj --launch-profile http
```

En Development, la API queda en `http://localhost:5293`, Swagger en `http://localhost:5293/swagger` y el estado de salud en `http://localhost:5293/health`.

El perfil `http` establece Development mediante `launchSettings.json`. Si se ejecuta directamente el binario sin definir entorno, la aplicación también usa Development de forma predeterminada para facilitar la ejecución local. Para despliegues se debe definir explícitamente `ASPNETCORE_ENVIRONMENT=Production`.

La aplicación no aplica migraciones automáticamente por defecto (`Database:Migrate=false`). Para una base nueva, configura primero PostgreSQL y ejecuta la sección [Migraciones](#aplicar-migraciones-al-arrancar).

## Ejecutar desde Rider

1. Abre `canchero.slnx` en Rider.
2. Selecciona el proyecto/perfil `Presentation` con el perfil `http` o `https`.
3. Ejecuta con **Run**. Los perfiles de `src/Presentation/Properties/launchSettings.json` cargan `ASPNETCORE_ENVIRONMENT=Development`.
4. Abre `http://localhost:5293/swagger` para explorar la API.

Si Rider tiene una configuración de tipo **Executable** que ejecuta `Presentation` directamente, usa una configuración de tipo **.NET Project** o agrega `ASPNETCORE_ENVIRONMENT=Development` a las variables de entorno de esa configuración. En Development se carga `appsettings.Development.json` y también los User Secrets del proyecto.

## Configuración

La configuración se carga de JSON, User Secrets en Development y variables de entorno. Las variables de entorno tienen prioridad sobre los archivos JSON. Para nombres anidados se usa `__`, por ejemplo `ConnectionStrings__DefaultConnection` equivale a `ConnectionStrings:DefaultConnection`.

### Desarrollo local

El archivo `src/Presentation/appsettings.Development.json` incluye una cadena local sin contraseña:

```text
Host=localhost;Port=5432;Database=canchero;Username=postgres
```

Esta conexión funciona si PostgreSQL permite autenticación local sin contraseña. Si tu PostgreSQL exige contraseña, guárdala como User Secret. El proyecto ya tiene un `UserSecretsId` configurado.

Desde la raíz del repositorio, en Bash:

```bash
read -rsp "Contraseña de PostgreSQL: " PG_PASS; echo
dotnet user-secrets --project src/Presentation set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=canchero;Username=postgres;Password=${PG_PASS}"
unset PG_PASS
```

En Development, si `Jwt:SecretKey` no está configurado, la API genera una clave aleatoria al arrancar. No hace falta establecerla para ejecutar localmente. Como esa clave es temporal, los tokens emitidos dejan de ser válidos al reiniciar el proceso. Si quieres una clave local persistente, puedes guardarla como User Secret:

```bash
dotnet user-secrets --project src/Presentation set "Jwt:SecretKey" "$(openssl rand -base64 48)"
```

No guardes contraseñas ni secretos reales en `appsettings*.json` ni en el repositorio. El archivo `src/Presentation/CONFIGURACION.md` contiene un resumen de configuración adicional.

### Variables de configuración

| Clave | Obligatoria | Descripción |
|---|---:|---|
| `ConnectionStrings__DefaultConnection` | Sí en entornos distintos a Development | Cadena de conexión PostgreSQL. En Development hay un valor local predeterminado sin contraseña. |
| `Jwt__SecretKey` | Sí fuera de Development | Clave de firma JWT de al menos 32 caracteres. En Development se genera temporalmente si falta. |
| `Jwt__Issuer` | No | Emisor esperado del token. Predeterminado: `Canchero`. |
| `Jwt__Audience` | No | Audiencia esperada del token. Predeterminado: `Canchero`. |
| `Jwt__ExpirationMinutes` | No | Vigencia del token en minutos. Predeterminado: `60`. |
| `Database__Migrate` | No | Si es `true`, aplica migraciones y ejecuta el seeding al iniciar. Predeterminado: `false`. |
| `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, ... | Según el cliente web | Orígenes permitidos por CORS. Development incluye `localhost:3000` y `localhost:5173` en HTTP y HTTPS. |
| `Swagger__Enabled` | No | Habilita Swagger. Está habilitado en Development y deshabilitado en la configuración base. |
| `Geocoding__BaseUrl` | No | URL HTTP(S) del proveedor de geocodificación. Predeterminada: Nominatim. |
| `Geocoding__UserAgent` | No | User-Agent enviado a Nominatim. |
| `Geocoding__CountryCodes` | No | Códigos de país para filtrar geocodificación. Predeterminado: `co`. |
| `Geocoding__CacheMinutes` | No | Duración de caché de resultados. Predeterminado: `60`. |
| `Geocoding__TimeoutSeconds` | No | Tiempo máximo de espera del proveedor. Predeterminado: `10`. |
| `Reservations__PendingExpirationMinutes` | No | Minutos que una reserva puede seguir `Pending` antes de cancelarse automáticamente. Predeterminado: `30`. |
| `Reservations__SweepIntervalSeconds` | No | Segundos entre cada revisión de reservas vencidas. Predeterminado: `60`. |
| `RateLimiting__SearchPermitsPerMinute` | No | Búsquedas de canchas permitidas por usuario y minuto. Predeterminado: `30`. |
| `Admin__Email`, `Admin__Password`, `Admin__FirstName`, `Admin__LastName` | No | Si se establecen todos, crea el administrador inicial durante el seeding. Deben configurarse como secretos. |

Para permitir un frontend, configura solo sus orígenes concretos. Ejemplo para una instancia productiva:

```text
Cors__AllowedOrigins__0=https://app.ejemplo.com
Cors__AllowedOrigins__1=https://admin.ejemplo.com
```

No incluyas rutas ni `/` final en el origen; el origen se compone de esquema, host y puerto cuando corresponda.

### Producción

En producción define explícitamente, mediante el gestor de secretos de la plataforma o variables de entorno:

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=<cadena PostgreSQL real>
Jwt__SecretKey=<secreto aleatorio de al menos 32 caracteres>
Cors__AllowedOrigins__0=<origen frontend permitido>
```

También configura HTTPS en el servidor o proxy inverso. La redirección HTTPS está activa fuera de Development. No uses el secreto temporal de Development en producción.

## Base de datos, migraciones y datos iniciales

La persistencia usa PostgreSQL y Entity Framework Core. Las entidades de dominio y las tablas de Identity comparten `AppDbContext`. La migración inicial está en `src/Infrastructure/Migrations`.

### Crear la base de datos

Crea primero una base PostgreSQL llamada `canchero` (o cambia el nombre en la cadena de conexión). Por ejemplo, desde `psql` con un usuario autorizado:

```sql
CREATE DATABASE canchero;
```

### Aplicar migraciones al arrancar

La opción es `false` por defecto. Para Development, puedes activarla solo para esa ejecución:

```bash
Database__Migrate=true dotnet run --project src/Presentation/Presentation.csproj --launch-profile http
```

O guardar la opción para el entorno local:

```bash
dotnet user-secrets --project src/Presentation set "Database:Migrate" "true"
```

Con migración y seeding habilitados, la aplicación crea el esquema, los roles (`Customer`, `VenueOwner`, `Admin`) y, si se configuraron las cuatro claves `Admin:*`, una cuenta administradora inicial. Si no se configura administrador, los roles se crean igualmente.

### Aplicar migraciones por separado

Para gestionar el esquema fuera del arranque de la API se puede usar EF Core CLI. La factory de diseño de `Infrastructure` requiere la variable `CANCHERO_CONNECTION`:

```bash
export CANCHERO_CONNECTION="Host=localhost;Port=5432;Database=canchero;Username=postgres;Password=${PG_PASS}"
dotnet ef database update --project src/Infrastructure/Infrastructure.csproj --startup-project src/Presentation/Presentation.csproj
unset CANCHERO_CONNECTION
```

El comando `dotnet ef` requiere que esté instalada la herramienta EF Core. No habilites a la vez varios procesos de migración concurrentes contra la misma base.

## Autenticación y permisos

La autenticación usa ASP.NET Core Identity para las cuentas y JWT Bearer para proteger la API. El login devuelve un token que se envía en cada solicitud protegida:

```http
Authorization: Bearer <token>
```

En Swagger, pulsa **Authorize** y pega el token (Swagger añade el esquema Bearer). Las rutas de autenticación y `/health` son anónimas; las rutas de usuarios, sedes, canchas y reservas requieren un JWT válido.

Los roles disponibles son:

- `Customer`: puede reservar y consultar/gestionar sus propias reservas.
- `VenueOwner`: puede reservar y administrar sus sedes y canchas.
- `Admin`: puede consultar o gestionar recursos según las reglas de cada operación. No puede crear reservas.

El registro público solo admite `Customer` y `VenueOwner`; no permite registrar `Admin`. El actor para cada operación protegida se toma del identificador del claim del token, no del `ActorUserId` enviado en el cuerpo.

En reglas concretas, un administrador puede gestionar sedes como administrador; ciertas operaciones de canchas exigen que el actor sea propietario de la sede. Consulta la tabla de endpoints y ten en cuenta las reglas descritas en cada caso.

La política de contraseñas de Identity exige mínimo 8 caracteres, al menos una mayúscula y una minúscula, y un dígito. Se permiten hasta 5 intentos fallidos antes del bloqueo temporal de la cuenta.

## API HTTP

Las enumeraciones se envían como texto JSON, por ejemplo `"VenueOwner"`, `"Football5"`, `"COP"` o `"Pending"`.

Las respuestas de creación, actualización y consulta devuelven objetos JSON. El registro devuelve `204 No Content`; el login devuelve `200 OK` con `token`, `expiresAt` y `user`.

| Método | Ruta | Autenticación | Descripción |
|---|---|---:|---|
| `GET` | `/health` | No | Comprueba que el proceso HTTP responde; no verifica la conexión a PostgreSQL. |
| `POST` | `/api/auth/register` | No | Registra un usuario `Customer` o `VenueOwner`. |
| `POST` | `/api/auth/login` | No | Valida credenciales y devuelve token JWT. |
| `GET` | `/api/users/{id}` | Sí | Consulta el perfil propio; Admin puede consultar otro perfil. |
| `PUT` | `/api/users/{id}/email` | Sí | Cambia el email propio; Admin puede cambiar el de otro usuario. |
| `POST` | `/api/venues` | Sí | Crea una sede. Requiere `VenueOwner` o Admin. |
| `GET` | `/api/venues/{id}` | Sí | Consulta una sede. |
| `GET` | `/api/venues?ownerId={id}` | Sí | Lista sedes propias o las de otro propietario si el actor es Admin. Sin `ownerId`, consulta las propias. |
| `PUT` | `/api/venues/{id}` | Sí | Actualiza una sede; propietario o Admin. |
| `PATCH` | `/api/venues/{id}/status?isActive=true` | Sí | Activa o desactiva una sede; propietario o Admin. |
| `POST` | `/api/courts` | Sí | Crea una cancha en una sede propia. Esta operación exige ser el propietario de la sede; Admin no omite esa regla. |
| `GET` | `/api/courts/{id}` | Sí | Consulta una cancha. |
| `GET` | `/api/courts/search` | Sí | Busca canchas cercanas y aplica filtros. |
| `PUT` | `/api/courts/{id}` | Sí | Actualiza una cancha; propietario de la sede o Admin. |
| `PATCH` | `/api/courts/{id}/status` | Sí | Cambia estado operativo; propietario de la sede o Admin. |
| `POST` | `/api/reservations` | Sí | Crea una reserva pendiente. |
| `GET` | `/api/reservations/mine` | Sí | Lista las reservas del actor. |
| `GET` | `/api/reservations/court/{courtId}` | Sí | Lista reservas de una cancha; propietario de la sede o Admin. |
| `GET` | `/api/reservations/court/{courtId}/availability?day={ISO-8601}` | Sí | Consulta franjas disponibles para una fecha. |
| `POST` | `/api/reservations/{id}/cancel` | Sí | Cancela una reserva propia; propietario de sede o Admin también puede cancelarla. |
| `POST` | `/api/reservations/{id}/confirm` | Sí | Confirma una reserva pendiente; propietario de sede o Admin. |
| `POST` | `/api/reservations/{id}/complete` | Sí | Completa una reserva confirmada después de su horario; propietario de sede o Admin. |
| `POST` | `/api/reservations/{id}/no-show` | Sí | Marca una reserva confirmada como inasistencia después de su inicio; propietario de sede o Admin. |

### Errores

Los errores se devuelven como `application/problem+json` (RFC 7807) mediante un manejador global de excepciones:

| Código | Cuándo ocurre |
|---:|---|
| `400` | Datos inválidos (validación de campos o invariantes del dominio). En validaciones de campos, `errors` agrupa los mensajes por propiedad. |
| `401` | Credenciales inválidas al iniciar sesión, o token ausente/inválido. |
| `403` | El usuario autenticado no tiene permiso para la operación. |
| `404` | El recurso solicitado no existe. |
| `409` | Conflicto con el estado actual, por ejemplo una franja ya reservada o un email duplicado. |
| `422` | Se incumple una regla de negocio, por ejemplo una transición de estado de reserva no permitida. |
| `429` | Se superó el límite de búsquedas por minuto; la respuesta incluye `Retry-After`. |
| `502` | Falla el proveedor externo de geocodificación (Nominatim). |
| `500` | Error no previsto. En Development incluye el detalle de la excepción; fuera de Development, un mensaje genérico. |

### Parámetros de búsqueda de canchas

`GET /api/courts/search` admite estos query parameters:

| Parámetro | Descripción |
|---|---|
| `locationText` | Lugar que se geocodifica con Nominatim (3 a 200 caracteres). Debe usarse en lugar de coordenadas. |
| `latitude`, `longitude` | Coordenadas; se deben proporcionar juntas y no junto con `locationText`. |
| `radiusKm` | Radio de búsqueda, entre `0.01` y `100`; predeterminado `10`. |
| `sportType` | Enum `SportType`. |
| `surfaceType` | Enum `SurfaceType`. |
| `isIndoor` | Filtra canchas cubiertas (`true`) o al aire libre (`false`). |
| `maxPricePerHour` | Precio máximo por hora. |
| `priceCurrency` | Moneda del filtro (`COP` o `USD`); predeterminada `COP`. |
| `slotStart`, `slotEnd` | Fechas ISO-8601 para filtrar disponibilidad; deben enviarse ambas y el inicio debe ser futuro. |
| `page`, `pageSize` | Paginación; predeterminados `1` y `10`, tamaño máximo `50`. |

Ejemplo: `/api/courts/search?latitude=4.6097&longitude=-74.0817&radiusKm=10&sportType=Football5&page=1&pageSize=10`.

### Paginación y fechas

Las listas paginadas devuelven `items`, `page`, `pageSize`, `totalCount` y `totalPages`. Los parámetros de paginación aceptan `page >= 1` y `pageSize` entre 1 y 50.

Las fechas se envían como ISO-8601 con zona horaria/offset, por ejemplo `2026-11-24T09:00:00-05:00`; el instante se convierte a la zona horaria de la sede para validar su horario. En disponibilidad, `day` indica la fecha de calendario consultada (se usa la fecha tal como llega, sin convertirla); las franjas se calculan en la zona horaria de la sede y la respuesta las expresa con el mismo offset que se envió en `day`.

## Ejemplos de uso

Los siguientes ejemplos usan Bash, `curl` y la API en `http://localhost:5293`.

### 1. Registrar una cuenta

```bash
curl -i -X POST http://localhost:5293/api/auth/register \
  -H 'Content-Type: application/json' \
  -d '{
    "firstName": "Ana",
    "lastName": "Gómez",
    "email": "ana@example.com",
    "phoneNumber": "+573001234567",
    "password": "ClaveSegura123",
    "role": "VenueOwner"
  }'
```

El registro devuelve `204 No Content`. Para poder iniciar sesión, las migraciones y el seeding de roles deben haberse ejecutado.

### 2. Iniciar sesión y guardar el token

```bash
TOKEN=$(curl -s -X POST http://localhost:5293/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"email":"ana@example.com","password":"ClaveSegura123"}' \
  | python3 -c 'import json,sys; print(json.load(sys.stdin)["token"])')
```

El login devuelve el token, su fecha de expiración y los datos básicos del usuario.

### 3. Crear una sede

```bash
curl -i -X POST http://localhost:5293/api/venues \
  -H "Authorization: Bearer $TOKEN" \
  -H 'Content-Type: application/json' \
  -d '{
    "name": "Canchas del Parque",
    "description": "Complejo deportivo",
    "street": "Calle 10 # 20-30",
    "city": "Bogotá",
    "department": "Cundinamarca",
    "country": "Colombia",
    "latitude": 4.6097,
    "longitude": -74.0817,
    "phoneNumber": "+573001234567",
    "openingHour": 8,
    "openingMinute": 0,
    "closingHour": 22,
    "closingMinute": 0,
    "timeZoneId": "America/Bogota"
  }'
```

`timeZoneId` es opcional (predeterminado `America/Bogota`). La respuesta contiene el `id` de la sede; úsalo como `venueId` al crear una cancha.

### 4. Crear una cancha

```bash
curl -i -X POST http://localhost:5293/api/courts \
  -H "Authorization: Bearer $TOKEN" \
  -H 'Content-Type: application/json' \
  -d '{
    "venueId": "<ID-DE-SEDE>",
    "name": "Cancha 1",
    "sportType": "Football5",
    "surfaceType": "SyntheticGrass",
    "capacity": 10,
    "pricePerHour": 120000,
    "currency": "COP",
    "isIndoor": false
  }'
```

### 5. Crear una reserva

```bash
curl -i -X POST http://localhost:5293/api/reservations \
  -H "Authorization: Bearer $TOKEN" \
  -H 'Content-Type: application/json' \
  -d '{
    "courtId": "<ID-DE-CANCHA>",
    "slotStart": "2026-11-25T10:00:00-05:00",
    "slotEnd": "2026-11-25T11:00:00-05:00"
  }'
```

La reserva se crea en estado `Pending`. El actor autenticado se asigna desde el JWT.

## Modelo y reglas de negocio

- **Usuario:** perfil de dominio vinculado a una cuenta Identity por el mismo identificador. Roles: `Customer`, `VenueOwner`, `Admin`.
- **Sede (Venue):** pertenece a un usuario propietario e incluye dirección, coordenadas, teléfono, horario, zona horaria (`timeZoneId`, identificador IANA; `America/Bogota` por defecto) y estado activo. El horario de apertura y cierre siempre se interpreta en la zona horaria de la sede, sin importar la zona del servidor ni el offset con el que el cliente envíe las fechas.
- **Cancha (Court):** pertenece a una sede, define deporte, superficie, capacidad, precio por hora, moneda, si es cubierta y estado (`Active`, `Inactive`, `Maintenance`).
- **Reserva (Reservation):** relaciona usuario y cancha, intervalo de tiempo, valor total, moneda, estado y motivo opcional de cancelación.
- Una reserva nueva empieza en `Pending`; solo una cancha reservable en una sede activa y abierta puede reservarse.
- Las reservas son de horas completas, contadas desde la hora de apertura de la sede (la misma cuadrícula que devuelve la disponibilidad), y duran como máximo 4 horas.
- Un usuario puede tener como máximo 5 reservas `Pending` a la vez. Una reserva que sigue `Pending` pasados `Reservations:PendingExpirationMinutes` minutos, o cuyo horario ya empezó, se cancela automáticamente con el motivo "Expirada por falta de confirmación.".
- Una reserva no puede cancelarse después de que su horario terminó.
- No se permiten reservas activas que se solapen para la misma cancha. La base de datos tiene una restricción de exclusión para proteger contra concurrencia.
- La disponibilidad se calcula en franjas de una hora dentro del horario de la sede.
- Transiciones de reserva: `Pending` puede confirmarse o cancelarse; `Confirmed` puede cancelarse, completarse tras finalizar o marcarse como inasistencia tras su inicio.
- El cambio de email y el registro coordinan Identity y el perfil de dominio dentro de una transacción.
- La geocodificación por texto usa Nominatim con caché y limita las consultas para respetar el proveedor. La búsqueda de canchas filtra las sedes por un rectángulo geográfico en la base de datos antes de calcular la distancia exacta, y está limitada por usuario (`RateLimiting:SearchPermitsPerMinute`).

## Estructura de la solución

```text
src/
├── Domain/          Entidades, enums, objetos de valor y reglas de dominio.
├── Application/     Casos de uso, DTOs, validadores e interfaces.
├── Infrastructure/  PostgreSQL, EF Core, Identity, migraciones y Nominatim.
└── Presentation/    ASP.NET Core, controladores, autenticación y Swagger.
```

La solución abierta en Rider es `canchero.slnx`. La API HTTP está en `src/Presentation`.

## Solución de problemas

### Falta `ConnectionStrings:DefaultConnection`

Comprueba que el proceso se ejecuta en Development para que cargue `appsettings.Development.json`, o configura `ConnectionStrings__DefaultConnection`/User Secrets. En Rider usa el perfil `http` o `https` del proyecto `Presentation`; una configuración **Executable** directa puede ignorar `launchSettings.json`.

### Falla la conexión a PostgreSQL

Verifica que PostgreSQL esté iniciado, que exista la base `canchero`, que el host/puerto sean correctos y que el usuario tenga acceso. Si PostgreSQL exige contraseña, configura la conexión en User Secrets para Development o en el gestor de secretos del despliegue.

### No existen roles o tablas

`Database:Migrate` está desactivado por defecto. Habilita migraciones durante el arranque o aplícalas con EF Core CLI. Si el esquema ya existe pero faltan roles, vuelve a ejecutar el seeding controlado con `Database:Migrate=true`.

### JWT inválido después de reiniciar

En Development, la clave se genera de nuevo al arrancar si no se configuró una persistente. El token anterior deja de ser válido. Inicia sesión otra vez o guarda `Jwt:SecretKey` como User Secret.

### El navegador bloquea una llamada por CORS

Agrega el origen exacto del frontend a `Cors:AllowedOrigins` (en Development, modifica la lista de `appsettings.Development.json`; en despliegues, usa `Cors__AllowedOrigins__0`, etc.). Los clientes HTTP como `curl` no están sujetos a CORS.

### El endpoint de búsqueda no encuentra una ubicación textual

`locationText` usa el proveedor externo Nominatim y está limitado por país a `co` de forma predeterminada. Comprueba la conexión a Internet, el nombre del lugar y la configuración `Geocoding:*`. También puedes buscar por coordenadas sin llamar al geocodificador.

## Estado de pruebas

La solución no contiene actualmente un proyecto de pruebas automatizadas. La compilación de la API se puede comprobar con:

```bash
dotnet build canchero.slnx
```
