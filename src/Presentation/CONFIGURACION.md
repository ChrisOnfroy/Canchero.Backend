# Configuración de Presentation

La aplicación no guarda credenciales de producción en `appsettings.json`. En Development hay una cadena local sin contraseña en `appsettings.Development.json`; PostgreSQL debe permitir esa conexión local. Para cualquier otro entorno, configura:

- `ConnectionStrings__DefaultConnection`: cadena PostgreSQL de la aplicación.
- `Jwt__SecretKey`: secreto aleatorio de al menos 32 caracteres (recomendado: 64 bytes aleatorios codificados en Base64).
- `Database__Migrate`: `true` para aplicar migraciones y ejecutar el seeding al arrancar; `false` para despliegues donde las migraciones se ejecutan por separado.
- `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, etc.: lista de orígenes frontend permitidos. En producción deben ser los orígenes reales.

Ejemplo para desarrollo local con valores sensibles externos (genera un secreto distinto para cada entorno):

```bash
export ConnectionStrings__DefaultConnection='Host=localhost;Port=5432;Database=canchero;Username=postgres;Password=<tu-clave>'
export Jwt__SecretKey="$(openssl rand -base64 48)"
export Database__Migrate=true
dotnet run --project src/Presentation
```

También puedes guardar ambos valores en User Secrets, que se cargan automáticamente en Development:

```bash
dotnet user-secrets --project src/Presentation set 'Jwt:SecretKey' "$(openssl rand -base64 48)"
dotnet user-secrets --project src/Presentation set 'ConnectionStrings:DefaultConnection' 'Host=localhost;Port=5432;Database=canchero;Username=postgres;Password=<tu-clave>'
```

Al ejecutar el binario directamente, configura `ASPNETCORE_ENVIRONMENT=Development` para cargar la configuración de desarrollo y User Secrets. En otros entornos, pasa los valores sensibles por variables de entorno o un gestor de secretos.

Las variables de entorno sobreescriben la configuración JSON de ASP.NET Core. Guarda las credenciales en el gestor de secretos del entorno de despliegue; no las agregues al repositorio.
