# Autenticación Multi-Tenant - Decisión de Arquitectura

## Decisión de Estrategia

### Cookie Authentication vs JWT

**Decisión: Cookie Authentication con ASP.NET Core Identity**

**Justificación:**
1. **MVC + AdminLTE**: El sistema usa Razor Pages/MVC Views, no es API-first
2. **Simplicidad**: Cookie auth es más simple para MVC tradicional
3. **Seguridad**: Cookies HttpOnly son más seguras que JWT en localStorage
4. **CSRF Protection**: ASP.NET Core Identity incluye protección CSRF automática
5. **Session Management**: Más fácil invalidar sesiones (logout real)
6. **AdminLTE**: Funciona mejor con cookie-based auth

**Si fuera API-first o SPA, usaríamos JWT + Refresh Tokens.**

## TenantResolver - Decisión

### Subdominio vs Header

**Decisión para MVP: Header (X-Tenant-Id) con fallback a Subdominio**

**Justificación MVP (Header):**
1. **Desarrollo Local**: No requiere configuración DNS
2. **Testing**: Más fácil de probar
3. **Flexibilidad**: Funciona en cualquier entorno
4. **Rápido de implementar**: No requiere infraestructura adicional

**Para Producción (Subdominio):**
- Mejor UX (tenant1.miapp.com)
- Más profesional
- SEO friendly
- Se implementará después del MVP

**Implementación:**
- Prioridad 1: Header `X-Tenant-Id`
- Prioridad 2: Subdominio (si header no existe)
- Prioridad 3: Claim del token (si está autenticado)

## Modelo de Datos

### Tablas Necesarias

**1. AspNetUsers (Identity)**
- Id (string/Guid)
- Email
- PasswordHash
- TenantId (FK a Tenants)
- IsActive
- FailedLoginAttempts
- LockoutEnd
- CreatedAt, UpdatedAt

**2. AspNetRoles (Identity)**
- Id
- Name (Owner, Admin, Marketer, Viewer)
- NormalizedName

**3. AspNetUserRoles (Identity)**
- UserId
- RoleId
- TenantId (IMPORTANTE: roles son por tenant)

**4. UserTenants (Custom)**
- UserId
- TenantId
- RoleId
- IsPrimary (si es el tenant principal del usuario)
- JoinedAt

**Nota:** Un usuario puede pertenecer a múltiples tenants con diferentes roles.

## Roles Definidos

1. **Owner**: Dueño del tenant, acceso total
2. **Admin**: Administrador del tenant, casi todo excepto eliminar tenant
3. **Marketer**: Puede crear/editar campañas y contenido
4. **Viewer**: Solo lectura

## Flujo de Autenticación

1. Usuario ingresa email + password
2. Sistema valida credenciales
3. Sistema obtiene TenantId (header o subdominio)
4. Sistema valida que usuario pertenece a ese tenant
5. Sistema crea cookie de autenticación con:
   - UserId
   - TenantId
   - Roles (para ese tenant)
6. Sistema registra login en auditoría

## Seguridad

### Protección contra Brute Force

1. **Rate Limiting**: Máximo 5 intentos por IP en 15 minutos
2. **Account Lockout**: 5 intentos fallidos = lockout 15 minutos
3. **Logging**: Todos los intentos fallidos se registran

### Cookies Seguras

- HttpOnly: true (no accesible desde JavaScript)
- Secure: true (solo HTTPS en producción)
- SameSite: Strict
- Expiration: 24 horas (configurable)

## Middleware Order

1. Exception Handler
2. Security Headers
3. Tenant Resolver (antes de auth)
4. Authentication
5. Authorization
6. Consent Validation


