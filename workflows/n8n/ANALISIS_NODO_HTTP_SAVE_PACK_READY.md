# Análisis del Nodo "HTTP Request - Save Pack (Ready)"

## 📋 Resumen
Este documento analiza el nodo "HTTP Request - Save Pack (Ready)", qué datos debe recibir, de dónde vienen, cómo se pasan y si hay problemas en el flujo.

---

## 🔄 Flujo de Datos

### 1. Nodo Anterior: "Build Marketing Pack"
**Ubicación:** Líneas 549-561 del workflow

**Qué devuelve:**
```javascript
{
  json: {
    // Datos completos de componentes anteriores
    ...components,
    
    // Objeto completo del MarketingPack
    marketingPack: {
      id: "...",
      tenantId: "...",
      userId: "...",
      contentId: "...",
      campaignId: "...",
      strategy: "...",
      status: "Ready" | "Generated",
      version: 1,
      metadata: "...",
      copies: [...],
      assetPrompts: [...],
      // ... otros campos
    },
    
    // Datos cognitivos
    cognitiveDecision: {...},
    
    // ⚠️ IMPORTANTE: También pone los campos directamente en el nivel raíz
    id: "...",
    tenantId: "...",
    userId: "...",
    contentId: "...",
    campaignId: "...",
    strategy: "...",
    status: "Ready" | "Generated",
    version: 1,
    metadata: "...",
    copies: [...],
    assetPrompts: [...]
  }
}
```

**Observación:** El nodo "Build Marketing Pack" coloca los campos `requestData` tanto dentro de `marketingPack` como directamente en el nivel raíz del `json`.

---

### 2. Nodo Intermedio: "Check Requires Approval Final"
**Ubicación:** Líneas 629-658 del workflow

**Qué hace:**
- Evalúa: `$json.marketingPack.requiresApproval ?? $json.requiresApproval ?? true`
- Si es `true` → Va a "HTTP Request - Save Pack (Requires Approval)"
- Si es `false` → Va a "HTTP Request - Save Pack (Ready)" ✅

**Pasa los datos sin modificación:** El `$json` completo pasa al siguiente nodo.

---

### 3. Nodo Actual: "HTTP Request - Save Pack (Ready)"
**Ubicación:** Líneas 680-708 del workflow

**Qué recibe:**
```javascript
$json = {
  // Datos de componentes anteriores
  strategy: {...},
  copy: {...},
  visualPrompts: {...},
  analysis: {...},
  cognitiveDecision: {...},
  
  // Objeto completo del MarketingPack
  marketingPack: {
    id: "...",
    tenantId: "...",
    userId: "...",
    contentId: "...",
    // ...
  },
  
  // ⚠️ Campos también en el nivel raíz (duplicados)
  id: "...",
  tenantId: "...",
  userId: "...",
  contentId: "...",
  campaignId: "...",
  strategy: "...",
  status: "...",
  version: 1,
  metadata: "...",
  copies: [...],
  assetPrompts: [...]
}
```

**Qué debe enviar al backend:**
El backend espera (según `CreateMarketingPackRequest`):
```json
{
  "request": {
    "id": "guid | null",
    "tenantId": "guid (requerido)",
    "userId": "guid (requerido)",
    "contentId": "guid (requerido)",
    "campaignId": "guid | null",
    "strategy": "string",
    "status": "string",
    "version": "number",
    "metadata": "string | null",
    "copies": [
      {
        "id": "guid | null",
        "copyType": "string",
        "content": "string",
        "hashtags": "string | null",
        "suggestedChannel": "string | null",
        "publicationChecklist": "object | null"
      }
    ],
    "assetPrompts": [
      {
        "id": "guid | null",
        "assetType": "string",
        "prompt": "string",
        "negativePrompt": "string | null",
        "parameters": "object | null",
        "suggestedChannel": "string | null"
      }
    ]
  }
}
```

**Código actual (línea 695):**
```javascript
jsonBody: "={{ (() => { 
  try { 
    const json = $json || {}; 
    
    // ❌ PROBLEMA 1: Mezcla fuentes de datos
    // tenantId y userId vienen de Set Validated Data
    const tenantId = String($('Set Validated Data').item.json.body.tenantId || '').trim();
    const userId = String($('Set Validated Data').item.json.body.userId || '').trim();
    
    // contentId viene de $json (nivel raíz, de Build Marketing Pack)
    const contentId = String(json.contentId || '').trim();
    
    // ❌ PROBLEMA 2: Los demás campos vienen de $json (nivel raíz)
    // pero tenantId y userId NO vienen de $json
    const req = {
      id: json.id || null,
      tenantId: tenantId,  // ⚠️ De Set Validated Data
      userId: userId,      // ⚠️ De Set Validated Data
      contentId: contentId, // ✅ De $json
      campaignId: json.campaignId || null,
      strategy: String(json.strategy || ''),
      status: String(json.status || 'Ready'),
      version: Number(json.version || 1),
      metadata: cleanMetadata(json.metadata),
      copies: cleanArray(json.copies),
      assetPrompts: cleanArray(json.assetPrompts)
    };
    
    return { request: req };
  } catch (e) {
    return { request: { error: 'Error: ' + String(e.message) } };
  }
})() }}"
```

---

## ⚠️ Problemas Identificados

### Problema 1: Mezcla de Fuentes de Datos
- **`tenantId` y `userId`** se obtienen de `$('Set Validated Data').item.json.body`
- **`contentId` y demás campos** se obtienen de `$json` (nivel raíz, que viene de "Build Marketing Pack")

**Riesgo:** Si "Build Marketing Pack" coloca `tenantId` y `userId` en el nivel raíz (lo hace), pero el nodo HTTP Request los ignora y va a buscar a "Set Validated Data", puede haber inconsistencias si los valores difieren.

### Problema 2: Prioridad Inconsistente
El nodo "Build Marketing Pack" ya tiene la lógica para obtener `tenantId` y `userId` desde "Set Validated Data" y los coloca en el nivel raíz. El nodo HTTP Request debería usar esos valores directamente de `$json` en lugar de volver a buscarlos en "Set Validated Data".

### Problema 3: Falta de Validación de marketingPack
El código actual no valida si `$json.marketingPack` existe, aunque "Build Marketing Pack" sí lo crea. Si por alguna razón el objeto `marketingPack` no existe, el código no lo detectaría.

---

## ✅ Solución Recomendada

### Opción 1: Usar datos del nivel raíz (Recomendada)
Priorizar los campos del nivel raíz (`$json`), que ya vienen de "Build Marketing Pack" con la lógica correcta:

```javascript
jsonBody: "={{ (() => { 
  try { 
    const json = $json || {}; 
    const mp = json.marketingPack || {};  // Opcional: también verificar marketingPack
    
    // ✅ PRIORIZAR: $json (nivel raíz) que viene de Build Marketing Pack
    // ✅ FALLBACK: marketingPack si existe
    // ✅ ÚLTIMO RECURSO: Set Validated Data
    
    const tenantId = String(
      json.tenantId || 
      mp.tenantId || 
      $('Set Validated Data').item?.json?.body?.tenantId || 
      ''
    ).trim();
    
    const userId = String(
      json.userId || 
      mp.userId || 
      $('Set Validated Data').item?.json?.body?.userId || 
      ''
    ).trim();
    
    const contentId = String(
      json.contentId || 
      mp.contentId || 
      $('Set Validated Data').item?.json?.body?.contentId || 
      ''
    ).trim();
    
    // Validaciones estrictas
    if (!tenantId || tenantId === 'undefined' || tenantId === 'null' || tenantId === '00000000-0000-0000-0000-000000000000') {
      throw new Error('tenantId is required and must be a valid GUID. Found: ' + JSON.stringify(tenantId));
    }
    
    if (!userId || userId === 'undefined' || userId === 'null' || userId === '00000000-0000-0000-0000-000000000000') {
      throw new Error('userId is required and must be a valid GUID. Found: ' + JSON.stringify(userId));
    }
    
    if (!contentId || contentId === 'undefined' || contentId === 'null' || contentId === '00000000-0000-0000-0000-000000000000') {
      throw new Error('contentId is required and must be a valid GUID. Found: ' + JSON.stringify(contentId));
    }
    
    // Funciones helper para limpiar datos
    function cleanMetadata(md) {
      if (md === null || md === undefined) return null;
      if (typeof md === 'string') return md;
      try {
        return JSON.stringify(md);
      } catch {
        return null;
      }
    }
    
    function cleanArray(arr) {
      if (!Array.isArray(arr)) return [];
      try {
        return JSON.parse(JSON.stringify(arr));
      } catch {
        return [];
      }
    }
    
    // Construir request
    const req = {
      id: json.id || mp.id || null,
      tenantId: tenantId,
      userId: userId,
      contentId: contentId,
      campaignId: json.campaignId || mp.campaignId || null,
      strategy: String(json.strategy || mp.strategy || ''),
      status: String(json.status || mp.status || 'Ready'),
      version: Number(json.version || mp.version || 1),
      metadata: cleanMetadata(json.metadata || mp.metadata),
      copies: cleanArray(json.copies || mp.copies || []),
      assetPrompts: cleanArray(json.assetPrompts || mp.assetPrompts || [])
    };
    
    return { request: req };
  } catch (e) {
    return { 
      request: { 
        error: 'Error: ' + String(e.message), 
        stack: String(e.stack || '') 
      } 
    };
  }
})() }}"
```

### Opción 2: Usar marketingPack directamente (Alternativa)
Si queremos usar explícitamente el objeto `marketingPack`:

```javascript
const mp = $json.marketingPack || {};
if (!mp || Object.keys(mp).length === 0) {
  throw new Error('marketingPack is required but not found');
}

const req = {
  id: mp.id || null,
  tenantId: String(mp.tenantId || '').trim(),
  userId: String(mp.userId || '').trim(),
  contentId: String(mp.contentId || '').trim(),
  // ... resto de campos
};
```

---

## 📊 Comparación: "HTTP Request - Save Pack (Requires Approval)" vs "HTTP Request - Save Pack (Ready)"

### "HTTP Request - Save Pack (Requires Approval)" (Línea 661)
- **Usa:** `$json.marketingPack` directamente
- **Validación:** Verifica que `marketingPack` exista
- **Fuentes:** `mp.tenantId`, `mp.userId`, `mp.contentId` con fallbacks a `Set Validated Data`

### "HTTP Request - Save Pack (Ready)" (Línea 680)
- **Usa:** `$json` (nivel raíz) para la mayoría de campos
- **Validación:** No valida `marketingPack`
- **Fuentes:** Mezcla `Set Validated Data` (tenantId, userId) con `$json` (contentId y demás)

**Recomendación:** Hacer que ambos nodos usen la misma estrategia para consistencia.

---

## 🎯 Resumen de Cambios Recomendados

1. **Priorizar campos del nivel raíz (`$json`)** que vienen de "Build Marketing Pack"
2. **Agregar fallbacks** a `marketingPack` y luego a `Set Validated Data`
3. **Validar campos requeridos** con mensajes de error claros
4. **Mantener consistencia** con "HTTP Request - Save Pack (Requires Approval)"
5. **Mejorar manejo de errores** para facilitar debugging

---

## ✅ Verificación del Backend

El backend espera (según `CreateMarketingPackRequest`):
- ✅ `tenantId`: `Guid` (requerido, no puede ser `Guid.Empty`)
- ✅ `userId`: `Guid` (requerido, no puede ser `Guid.Empty`)
- ✅ `contentId`: `Guid` (requerido, no puede ser `Guid.Empty`)
- ✅ `campaignId`: `Guid?` (opcional, puede ser `null`)
- ✅ `strategy`: `string` (requerido, no puede ser vacío)
- ✅ `status`: `string?` (opcional, default: "Generated")
- ✅ `version`: `int` (default: 1)
- ✅ `metadata`: `string?` (opcional, puede ser `null`)
- ✅ `copies`: `List<GeneratedCopyRequest>?` (opcional, puede ser `null`)
- ✅ `assetPrompts`: `List<MarketingAssetPromptRequest>?` (opcional, puede ser `null`)

**Todos los campos están correctamente mapeados en el código actual**, pero la inconsistencia en las fuentes de datos puede causar problemas.

