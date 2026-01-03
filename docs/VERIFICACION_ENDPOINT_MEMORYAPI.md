# Verificación del Endpoint MemoryApi/context

## Estado del Endpoint

### ✅ Endpoint Existe en el Código

**Controlador:** `src/AutonomousMarketingPlatform.Web/Controllers/Api/MemoryApiController.cs`

**Ruta Configurada:**
```csharp
[ApiController]
[Route("api/[controller]")]  // Se traduce a /api/MemoryApi
[AllowAnonymous]
public class MemoryApiController : ControllerBase
{
    [HttpGet("context")]  // Ruta completa: /api/MemoryApi/context
    public async Task<IActionResult> GetMemoryContext(...)
}
```

**Ruta Completa:** `GET /api/MemoryApi/context`

### Endpoints Disponibles en MemoryApiController

1. **GET /api/MemoryApi/context** (línea 161)
   - Parámetros: `tenantId` (requerido), `userId` (opcional), `campaignId` (opcional), `memoryType` (opcional)
   - Retorna: `MemoryContextResponse`

2. **GET /api/MemoryApi** (línea 50)
   - Parámetros: `tenantId` (requerido), `memoryType` (opcional), `tags` (opcional), `limit` (opcional)
   - Retorna: `List<MarketingMemoryDto>`

3. **POST /api/MemoryApi/save** (línea 450)
   - Body: `SaveMemoryRequest`
   - Retorna: `MarketingMemoryDto`

---

## Referencias en Workflow n8n

**Total de referencias:** 6

1. **HTTP Request - Load Marketing Memory** (línea 437)
   - URL: `https://autonomousmarketingplatform.onrender.com/api/MemoryApi/context`
   - Método: GET
   - Parámetros: `tenantId`, `userId`, `campaignId`

2. **HTTP Request - Load Preference Memory** (línea 530)
   - URL: `https://autonomousmarketingplatform.onrender.com/api/MemoryApi/context`
   - Método: GET
   - Parámetros: `tenantId`, `memoryType: "Preference"`

3. **HTTP Request - Load Performance Memory** (línea 560)
   - URL: `https://autonomousmarketingplatform.onrender.com/api/MemoryApi/context`
   - Método: GET
   - Parámetros: `tenantId`, `memoryType: "Learning"`

4. **HTTP Request - Load Constraint Memory** (línea 590)
   - URL: `https://autonomousmarketingplatform.onrender.com/api/MemoryApi/context`
   - Método: GET
   - Parámetros: `tenantId`, `memoryType: "Feedback"`

5. **HTTP Request - Load Pattern Memory** (línea 620)
   - URL: `https://autonomousmarketingplatform.onrender.com/api/MemoryApi/context`
   - Método: GET
   - Parámetros: `tenantId`, `memoryType: "Pattern"`

6. **HTTP Request - Save Override Memory** (línea 973)
   - URL: `https://autonomousmarketingplatform.onrender.com/api/MemoryApi/save`
   - Método: POST
   - Body: `SaveMemoryRequest`

---

## Posibles Problemas

### 1. Código No Desplegado en Render

**Síntoma:** El endpoint no existe en Render pero existe en el código local.

**Solución:**
- Verificar que el código esté actualizado en el repositorio
- Verificar que Render esté haciendo deploy del código más reciente
- Verificar los logs de deploy en Render

### 2. Problema con el Routing

**Síntoma:** El endpoint existe pero no responde correctamente.

**Verificación:**
- El controlador usa `[Route("api/[controller]")]` que se traduce a `/api/MemoryApi`
- El método usa `[HttpGet("context")]` que agrega `/context` a la ruta
- La ruta completa debería ser: `/api/MemoryApi/context`

**Solución:**
- Verificar que `app.MapControllers()` esté llamado en `Program.cs` (línea 561)
- Verificar que no haya conflictos de routing

### 3. Problema con la Configuración de Render

**Síntoma:** El endpoint no está accesible desde fuera.

**Verificación:**
- Verificar que el puerto esté correctamente configurado
- Verificar que las variables de entorno estén configuradas
- Verificar los logs de Render para errores de inicio

---

## Pruebas Recomendadas

### 1. Verificar que el Endpoint Existe Localmente

```bash
curl -X GET "http://localhost:5000/api/MemoryApi/context?tenantId=YOUR_TENANT_ID"
```

### 2. Verificar que el Endpoint Existe en Render

```bash
curl -X GET "https://autonomousmarketingplatform.onrender.com/api/MemoryApi/context?tenantId=YOUR_TENANT_ID"
```

### 3. Verificar el Health Check

```bash
curl -X GET "https://autonomousmarketingplatform.onrender.com/"
```

---

## Acciones Recomendadas

1. **Verificar el Deploy en Render:**
   - Revisar los logs de deploy
   - Verificar que el código esté actualizado
   - Verificar que no haya errores de compilación

2. **Verificar el Routing:**
   - Confirmar que `app.MapControllers()` esté en `Program.cs`
   - Verificar que no haya conflictos de rutas

3. **Probar el Endpoint Directamente:**
   - Usar curl o Postman para probar el endpoint
   - Verificar los logs del servidor en Render

---

**Última verificación:** 2025-01-01  
**Estado:** Endpoint existe en código, necesita verificación de deploy en Render

