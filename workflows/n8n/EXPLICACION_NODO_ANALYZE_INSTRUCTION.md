# Explicación: Nodo "OpenAI - Analyze Instruction (Cognitive)"

## 🎯 Propósito Principal

Este nodo es el **primer paso de análisis cognitivo** del workflow. Su función es **analizar la instrucción del usuario** usando OpenAI y extraer información estructurada que será usada por los nodos posteriores.

---

## 📥 **Entrada (Input)**

El nodo recibe datos de **"Consolidate Advanced Memory"** que incluyen:

```json
{
  "instruction": "Crear contenido de marketing para redes sociales con tono profesional...",
  "channels": ["instagram", "facebook", "tiktok"],
  "advancedMemory": {
    "preferenceMemory": {
      "preferredTone": "profesional",
      "preferredFormats": [],
      "preferredChannels": []
    },
    "performanceMemory": {
      "bestPerformingChannels": [],
      "avgCTR": 0,
      "avgEngagement": 0
    },
    "constraintMemory": {
      "restrictions": []
    }
  }
}
```

---

## 🔄 **Proceso**

### 1. **Extrae y Valida Datos:**
```javascript
const instruction = ($json.instruction || '').trim();
const preferences = $json.advancedMemory?.preferenceMemory || {};
const learnings = $json.advancedMemory?.performanceMemory || {};
const restrictions = $json.advancedMemory?.constraintMemory?.restrictions || [];
const channels = Array.isArray($json.channels) ? $json.channels : [];
```

### 2. **Construye el Prompt para OpenAI:**

**System Prompt:**
```
Eres un experto analista de marketing cognitivo. 
Analiza la instrucción del usuario y genera un JSON válido con esta estructura:

{
  "objective": "string",
  "tone": "string (profesional, casual, formal, amigable)",
  "urgency": "string (low, medium, high)",
  "contentType": "string (post, story, reel, video, carousel)",
  "targetAudience": "string",
  "keyMessages": ["string"],
  "hashtags": ["string"],
  "channels": ["string"]
}

Responde SOLO con el JSON, sin texto adicional.
```

**User Prompt:**
```
## Instrucción del Usuario:
[La instrucción completa del usuario]

## Contexto:
- Tono preferido: [del historial]
- Mejores canales: [del historial]
- Restricciones: [del historial]
- Canales solicitados (USAR ESTOS): [canales del body]

Analiza la instrucción y responde con el JSON.
```

### 3. **Envía a OpenAI:**
- Usa el modelo de OpenAI configurado
- Envía el prompt completo (system + user)
- Espera una respuesta JSON estructurada

---

## 📤 **Salida (Output)**

El nodo devuelve un JSON estructurado con el análisis de la instrucción:

```json
{
  "choices": [{
    "message": {
      "content": "{\"objective\":\"Aumentar engagement y visibilidad de marca\",\"tone\":\"profesional\",\"urgency\":\"medium\",\"contentType\":\"post\",\"targetAudience\":\"Profesionales y empresas\",\"keyMessages\":[\"Engagement\",\"Visibilidad\",\"Marca\"],\"hashtags\":[\"marketing\",\"redessociales\",\"engagement\"],\"channels\":[\"instagram\",\"facebook\",\"tiktok\"]}"
    }
  }]
}
```

---

## 🎯 **Qué Analiza la IA**

La IA analiza la instrucción del usuario y extrae:

1. **`objective`** - Objetivo principal de la campaña
   - Ejemplo: "Aumentar engagement y visibilidad de marca"

2. **`tone`** - Tono recomendado
   - Opciones: profesional, casual, formal, amigable
   - Basado en la instrucción y preferencias históricas

3. **`urgency`** - Nivel de urgencia
   - Opciones: low, medium, high
   - Detecta palabras clave como "urgente", "inmediato", etc.

4. **`contentType`** - Tipo de contenido sugerido
   - Opciones: post, story, reel, video, carousel
   - Basado en la instrucción y canales

5. **`targetAudience`** - Audiencia objetivo
   - Ejemplo: "Profesionales y empresas"
   - Extrae de menciones en la instrucción

6. **`keyMessages`** - Mensajes clave
   - Array de mensajes principales
   - Ejemplo: ["Engagement", "Visibilidad", "Marca"]

7. **`hashtags`** - Hashtags sugeridos
   - Array de hashtags relevantes
   - Basados en la instrucción y contexto

8. **`channels`** - Canales a usar
   - **IMPORTANTE:** Usa los canales del body, NO infiere de la instrucción
   - Ejemplo: ["instagram", "facebook", "tiktok"]

---

## 🔗 **Flujo en el Workflow**

```
Consolidate Advanced Memory
    ↓
OpenAI - Analyze Instruction (Cognitive)  ← ESTE NODO
    ↓
Parse Analysis
    ↓
OpenAI - Generate Strategy
    ↓
... (resto del flujo)
```

---

## 💡 **Por Qué es Importante**

Este nodo es **crítico** porque:

1. **Primera interpretación:** Es el primer paso donde la IA "entiende" qué quiere el usuario
2. **Estructura los datos:** Convierte texto libre en datos estructurados
3. **Base para todo:** El análisis resultante se usa en todos los nodos posteriores:
   - Para generar la estrategia
   - Para crear el copy
   - Para generar prompts visuales
   - Para tomar decisiones cognitivas

4. **Combina contexto:** No solo analiza la instrucción, sino que la combina con:
   - Preferencias históricas del usuario
   - Aprendizajes de rendimiento
   - Restricciones
   - Canales seleccionados

---

## ⚠️ **Limitaciones**

1. **Depende de la calidad de la instrucción:** Si la instrucción es vaga, el análisis será menos preciso
2. **Usa canales del body:** No infiere canales de la instrucción (por diseño)
3. **Requiere OpenAI:** Si OpenAI falla, todo el workflow se detiene

---

## 📊 **Ejemplo Real**

### Entrada:
```json
{
  "instruction": "Crear contenido de marketing para redes sociales con tono profesional y moderno, orientado a aumentar el engagement y la visibilidad de la marca en Instagram y LinkedIn.",
  "channels": ["instagram", "facebook", "tiktok"]
}
```

### Salida (después de Parse Analysis):
```json
{
  "analysis": {
    "objective": "Aumentar engagement y visibilidad de marca",
    "tone": "profesional",
    "urgency": "medium",
    "contentType": "post",
    "targetAudience": "Profesionales y empresas",
    "keyMessages": ["Engagement", "Visibilidad", "Marca"],
    "hashtags": ["marketing", "redessociales", "engagement"],
    "channels": ["instagram", "facebook", "tiktok"]
  }
}
```

---

## ✅ **Resumen**

**Este nodo es el "cerebro analítico" inicial** que:
- ✅ Lee la instrucción del usuario
- ✅ La analiza con contexto histórico
- ✅ Extrae información estructurada
- ✅ Prepara los datos para los siguientes pasos del workflow

**Sin este nodo, el workflow no sabría qué hacer con la instrucción del usuario.**

