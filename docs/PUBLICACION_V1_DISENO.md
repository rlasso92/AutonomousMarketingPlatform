# PROMPT 15 - Publicación V1: Diseño y Justificación

## Objetivo
Implementar un sistema de publicación a redes sociales con cola de trabajos (job queue) y scheduler, diseñado para escalar desde publicación manual hasta automatización completa.

## Estrategia Realista

### Limitaciones de APIs Sociales

1. **Instagram Graph API**
   - Requiere aprobación de Meta Business
   - Limitaciones de rate limiting estrictas
   - Necesita tokens de acceso complejos
   - Solo permite publicación programada (no inmediata)

2. **Facebook Page API**
   - Más accesible que Instagram
   - Requiere permisos de página
   - Rate limiting moderado
   - Mejor documentación

3. **TikTok API**
   - API muy restrictiva
   - Requiere aprobación especial
   - Limitaciones severas de rate limiting
   - Documentación limitada

### Decisión: Enfoque por Fases

#### Fase A (MVP - Implementación Inicial)
**Objetivo**: Generar paquetes listos para publicación manual sin perder tracking.

**Características**:
- Generar "paquete de publicación" con:
  - Copy completo (texto)
  - Hashtags formateados
  - Referencias a media (imágenes/videos)
  - Checklist de publicación por canal
  - Metadata completa
- Estado: `RequiresApproval`
- Permitir descarga del paquete como JSON/ZIP
- Tracking completo (quién descargó, cuándo, estado)

**Ventajas**:
- No requiere aprobación de APIs
- Funciona inmediatamente
- Permite validación manual
- Mantiene trazabilidad completa
- Base sólida para Fase B

#### Fase B (Automatización Real)
**Objetivo**: Integración real con APIs sociales.

**Priorización**:
1. **Facebook Page API** (prioridad alta)
   - Más accesible
   - Mejor documentación
   - Menos restricciones
2. **Instagram Graph API** (prioridad media)
   - Requiere aprobación Meta Business
   - Más complejo
3. **TikTok API** (prioridad baja)
   - Muy restrictivo
   - Requiere aprobación especial

**Características**:
- Conectores por canal
- Manejo de tokens de acceso
- Rate limiting inteligente
- Reintentos automáticos
- Webhooks para notificaciones

## Arquitectura

### Componentes Principales

1. **PublishingJob Entity**
   - Almacena estado y metadata
   - Payload completo en JSON
   - Tracking de reintentos

2. **PublishingJobService (HostedService)**
   - Procesa cola de trabajos
   - Scheduler para publicaciones programadas
   - Manejo de reintentos
   - Logging completo

3. **IPublishingAdapter (Interface)**
   - Contrato para adaptadores por canal
   - Implementaciones:
     - `ManualPublishingAdapter` (Fase A)
     - `FacebookPublishingAdapter` (Fase B)
     - `InstagramPublishingAdapter` (Fase B)
     - `TikTokPublishingAdapter` (Fase B)

4. **PublishingController**
   - Endpoints para UI
   - Generar publicaciones
   - Aprobar publicaciones
   - Descargar paquetes

### Flujo de Trabajo

#### Fase A (Manual)
```
1. Usuario genera publicación desde MarketingPack
2. Sistema crea PublishingJob con Status="Pending"
3. Background service procesa y genera paquete
4. Status cambia a "RequiresApproval"
5. Usuario descarga paquete
6. Usuario publica manualmente
7. Usuario marca como "Published" en UI
8. Status cambia a "Success"
```

#### Fase B (Automático)
```
1. Usuario genera publicación desde MarketingPack
2. Sistema crea PublishingJob con Status="Pending"
3. Background service procesa según scheduled_at
4. Adaptador correspondiente publica en red social
5. Status cambia a "Success" o "Failed"
6. Si Failed, reintento automático (hasta MaxRetries)
```

## Mecanismo de Reintentos

### Estrategia
- **MaxRetries**: 3 por defecto
- **Backoff exponencial**: 1min, 5min, 15min
- **Categorización de errores**:
  - **Transitorios**: Rate limiting, timeouts → Reintentar
  - **Permanentes**: Invalid token, invalid content → No reintentar
  - **Requiere acción**: RequiresApproval → Esperar aprobación

### Implementación
```csharp
if (error.IsTransient && retryCount < MaxRetries)
{
    var delay = TimeSpan.FromMinutes(Math.Pow(5, retryCount));
    await Task.Delay(delay);
    retryCount++;
    // Reintentar
}
```

## Seguridad y Compliance

1. **Tokens de API**: Encriptados en base de datos
2. **Rate Limiting**: Respetar límites de APIs
3. **Auditoría**: Log completo de todas las acciones
4. **Aprobaciones**: Requeridas para publicación automática (configurable)

## Escalabilidad

1. **Cola distribuida**: Preparado para Redis/Azure Service Bus
2. **Procesamiento paralelo**: Múltiples workers
3. **Priorización**: Jobs urgentes primero
4. **Monitoreo**: Health checks y métricas

## Métricas y Monitoreo

- Tasa de éxito/fallo por canal
- Tiempo promedio de procesamiento
- Reintentos promedio
- Publicaciones por día/semana
- Errores más comunes

## Conclusión

Esta estrategia permite:
- ✅ Lanzamiento rápido (Fase A)
- ✅ Valor inmediato para usuarios
- ✅ Base sólida para automatización
- ✅ Escalabilidad futura
- ✅ Manejo robusto de errores

