# Análisis: Instrucción vs Canales del Body

## 📋 Situación

### Payload Recibido:
```json
{
  "body": {
    "instruction": "Crear contenido de marketing para redes sociales con tono profesional y moderno, orientado a aumentar el engagement y la visibilidad de la marca en Instagram y LinkedIn.",
    "channels": ["instagram", "facebook", "tiktok"]
  }
}
```

### ⚠️ Conflicto Potencial:
- **Instrucción menciona:** "Instagram y LinkedIn"
- **Canales en body:** `["instagram", "facebook", "tiktok"]`
- **Diferencia:** La instrucción menciona "LinkedIn" pero el body tiene "facebook" y "tiktok"

---

## 🔍 Flujo de la Instrucción

### 1. **Nodo: Set Validated Data**
**Preserva:**
```json
{
  "body": {
    "instruction": "Crear contenido de marketing para redes sociales con tono profesional y moderno, orientado a aumentar el engagement y la visibilidad de la marca en Instagram y LinkedIn.",
    "channels": ["instagram", "facebook", "tiktok"]
  },
  "validatedData": {
    "instruction": "Crear contenido de marketing para redes sociales con tono profesional y moderno, orientado a aumentar el engagement y la visibilidad de la marca en Instagram y LinkedIn.",
    "channels": ["instagram", "facebook", "tiktok"]
  }
}
```

**✅ Canales preservados:** `["instagram", "facebook", "tiktok"]`

---

### 2. **Nodo: Normalize Memory**
**Preserva:**
```json
{
  "instruction": "Crear contenido de marketing para redes sociales con tono profesional y moderno, orientado a aumentar el engagement y la visibilidad de la marca en Instagram y LinkedIn.",
  "channels": ["instagram", "facebook", "tiktok"]
}
```

**✅ Canales preservados:** `["instagram", "facebook", "tiktok"]`

---

### 3. **Nodo: OpenAI - Analyze Instruction (Cognitive)**

**Recibe:**
```json
{
  "instruction": "Crear contenido de marketing para redes sociales con tono profesional y moderno, orientado a aumentar el engagement y la visibilidad de la marca en Instagram y LinkedIn.",
  "channels": ["instagram", "facebook", "tiktok"],
  "preferences": { ... },
  "learnings": { ... },
  "restrictions": []
}
```

**Prompt enviado a OpenAI:**
```
System: Eres un experto analista de marketing cognitivo...

User: Analiza la siguiente instrucción de marketing:

## Instrucción del Usuario:
Crear contenido de marketing para redes sociales con tono profesional y moderno, orientado a aumentar el engagement y la visibilidad de la marca en Instagram y LinkedIn.

## Contexto de Memoria:
- Preferencias: ...
- Aprendizajes: ...
- Restricciones: ...

### Canales Solicitados:
instagram, facebook, tiktok

Responde SOLO con el JSON del análisis, sin texto adicional.
```

**⚠️ IMPORTANTE:** El prompt incluye:
- La instrucción completa (que menciona "Instagram y LinkedIn")
- Los canales del body en "Canales Solicitados": `instagram, facebook, tiktok`

**Posible respuesta de OpenAI:**
```json
{
  "objective": "Aumentar engagement y visibilidad de marca",
  "tone": "profesional",
  "urgency": "medium",
  "contentType": "post",
  "targetAudience": "Profesionales y empresas",
  "keyMessages": ["Engagement", "Visibilidad", "Marca"],
  "hashtags": ["marketing", "redessociales", "engagement"],
  "channels": ["instagram", "linkedin"]  // ⚠️ OpenAI podría inferir esto de la instrucción
}
```

**⚠️ PROBLEMA POTENCIAL:** OpenAI podría inferir `["instagram", "linkedin"]` de la instrucción, ignorando los canales del body.

---

### 4. **Nodo: Parse Analysis**

**Recibe de OpenAI:**
```json
{
  "choices": [{
    "text": "{\"objective\":\"Aumentar engagement...\",\"channels\":[\"instagram\",\"linkedin\"]}"
  }]
}
```

**Código crítico:**
```javascript
channels: Array.isArray(normalizeMemoryData.channels) && normalizeMemoryData.channels.length > 0 
  ? normalizeMemoryData.channels  // ✅ PRIORIDAD 1: Canales del body
  : (Array.isArray(analysis.channels) ? analysis.channels : [])  // ⚠️ FALLBACK: Canales de OpenAI
```

**Procesamiento:**
1. `normalizeMemoryData.channels` = `["instagram", "facebook", "tiktok"]` (del body)
2. `analysis.channels` = `["instagram", "linkedin"]` (de OpenAI)
3. **Resultado:** Usa `normalizeMemoryData.channels` porque tiene `length > 0`

**✅ RESULTADO:**
```json
{
  "analysis": {
    "objective": "Aumentar engagement y visibilidad de marca",
    "tone": "profesional",
    "channels": ["instagram", "facebook", "tiktok"],  // ✅ Usa canales del body
    "originalInstruction": "Crear contenido de marketing para redes sociales con tono profesional y moderno, orientado a aumentar el engagement y la visibilidad de la marca en Instagram y LinkedIn."
  },
  "channels": ["instagram", "facebook", "tiktok"]  // ✅ Preservado del body
}
```

**✅ CORRECTO:** Los canales del body tienen prioridad sobre los que OpenAI infiere.

---

### 5. **Nodo: OpenAI - Generate Strategy**

**Recibe:**
```json
{
  "analysis": {
    "channels": ["instagram", "facebook", "tiktok"],  // ✅ Del body
    "originalInstruction": "Crear contenido de marketing para redes sociales con tono profesional y moderno, orientado a aumentar el engagement y la visibilidad de la marca en Instagram y LinkedIn."
  },
  "channels": ["instagram", "facebook", "tiktok"]  // ✅ Del body
}
```

**Prompt enviado a OpenAI:**
```javascript
const originalChannels = $('Set Validated Data').item?.json?.body?.channels || [];
const channels = Array.isArray(originalChannels) && originalChannels.length > 0 
  ? originalChannels  // ✅ ["instagram", "facebook", "tiktok"]
  : (Array.isArray($json.channels) ? $json.channels : []);
```

**Prompt:**
```
## Canales Solicitados (USAR ESTOS EXACTAMENTE):
instagram, facebook, tiktok

IMPORTANTE: Los canales especificados arriba son los que el usuario seleccionó. 
DEBES usar esos canales exactamente en el campo "channels" del JSON de respuesta, 
NO infieras otros canales basándote en la instrucción.
```

**✅ CORRECTO:** El prompt es explícito sobre usar los canales del body.

---

### 6. **Nodo: Parse Strategy**

**Código crítico:**
```javascript
channels: (() => { 
  const originalChannels = $('Set Validated Data').item?.json?.body?.channels || [];
  return Array.isArray(originalChannels) && originalChannels.length > 0 
    ? originalChannels  // ✅ PRIORIDAD 1: Canales del body
    : (Array.isArray(strategy.channels) && strategy.channels.length > 0 
        ? strategy.channels  // ⚠️ FALLBACK: Canales de OpenAI
        : (Array.isArray($('Parse Analysis').item.json.channels) && $('Parse Analysis').item.json.channels.length > 0 
            ? $('Parse Analysis').item.json.channels  // ⚠️ FALLBACK: Canales del análisis
            : []));
})()
```

**✅ CORRECTO:** Prioriza canales del body sobre los de OpenAI.

---

### 7. **Nodo: Build Marketing Pack**

**Código crítico:**
```javascript
const originalChannels = $('Set Validated Data').item?.json?.body?.channels || [];
const channels = Array.isArray(originalChannels) && originalChannels.length > 0 
  ? originalChannels  // ✅ PRIORIDAD 1: Canales del body
  : (Array.isArray(components.channels) && components.channels.length > 0 
      ? components.channels  // ⚠️ FALLBACK
      : (Array.isArray(strategy.channels) && strategy.channels.length > 0 
          ? strategy.channels  // ⚠️ FALLBACK
          : []));
```

**✅ CORRECTO:** Usa canales del body: `["instagram", "facebook", "tiktok"]`

---

## 📊 Resumen del Flujo

### Instrucción:
```
"Crear contenido de marketing para redes sociales con tono profesional y moderno, 
orientado a aumentar el engagement y la visibilidad de la marca en Instagram y LinkedIn."
```

### Canales del Body:
```json
["instagram", "facebook", "tiktok"]
```

### Procesamiento:

1. **OpenAI - Analyze Instruction:**
   - Recibe: Instrucción + Canales del body
   - Podría inferir: `["instagram", "linkedin"]` de la instrucción
   - Pero el prompt incluye: "Canales Solicitados: instagram, facebook, tiktok"

2. **Parse Analysis:**
   - Prioriza: `normalizeMemoryData.channels` = `["instagram", "facebook", "tiktok"]`
   - Ignora: `analysis.channels` = `["instagram", "linkedin"]` (si OpenAI lo infiere)

3. **OpenAI - Generate Strategy:**
   - Recibe: Canales del body explícitamente
   - Prompt dice: "USAR ESTOS EXACTAMENTE: instagram, facebook, tiktok"
   - OpenAI debe usar: `["instagram", "facebook", "tiktok"]`

4. **Parse Strategy:**
   - Prioriza: Canales del body
   - Resultado: `["instagram", "facebook", "tiktok"]`

5. **Build Marketing Pack:**
   - Usa: Canales del body
   - Resultado: `["instagram", "facebook", "tiktok"]`

6. **Prepare Publish Jobs:**
   - Crea jobs para: `["instagram", "facebook", "tiktok"]`
   - **NO crea jobs para:** `["linkedin"]` (aunque la instrucción lo menciona)

---

## ✅ Conclusión

### El flujo está CORRECTO:

1. **Los canales del body tienen PRIORIDAD** en todos los nodos
2. **La instrucción se usa solo para:**
   - Generar el objetivo
   - Determinar el tono
   - Crear el contenido
   - **NO para determinar los canales**

3. **Los canales finales son:** `["instagram", "facebook", "tiktok"]`
   - **NO** `["instagram", "linkedin"]` (aunque la instrucción lo menciona)

4. **El workflow publicará en:**
   - ✅ Instagram
   - ✅ Facebook
   - ✅ TikTok
   - ❌ LinkedIn (no está en los canales del body)

---

## 🎯 Recomendación

### Si quieres que el sistema use LinkedIn:
**Opción 1:** Incluir "linkedin" en el array de canales del body:
```json
{
  "channels": ["instagram", "facebook", "tiktok", "linkedin"]
}
```

**Opción 2:** El sistema podría detectar automáticamente canales mencionados en la instrucción, pero **actualmente NO lo hace** - solo usa los canales del body.

### Comportamiento Actual:
- ✅ **Respetado:** Canales del body `["instagram", "facebook", "tiktok"]`
- ✅ **Ignorado:** Canales mencionados en la instrucción ("LinkedIn")
- ✅ **Correcto:** El workflow publica solo en los canales del body

---

## 📝 Nota Final

El workflow está diseñado para **priorizar los canales del body sobre cualquier inferencia de la instrucción**. Esto es correcto porque:

1. Los canales del body son una **selección explícita del usuario**
2. La instrucción puede mencionar canales de forma **descriptiva o casual**
3. El sistema debe **respetar la selección explícita** del usuario

**El flujo está funcionando correctamente.**

