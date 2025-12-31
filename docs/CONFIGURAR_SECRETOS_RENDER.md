# 🔐 Configurar Secretos en Render

## 📋 Secretos Requeridos

Tu aplicación necesita estos secretos configurados en Render:

### 1. **OpenAI API Key** (`AI__OpenAI__ApiKey`)
- **Qué es**: Tu clave de API de OpenAI para usar GPT-4
- **Dónde obtenerla**: https://platform.openai.com/api-keys
- **Formato**: `sk-proj-...` (tu clave completa)

### 2. **Encryption Key** (`Encryption__Key`)
- **Qué es**: Clave de 32 caracteres para encriptar datos sensibles en la base de datos
- **Cómo generarla**: Debe ser exactamente 32 caracteres (puede ser alfanumérica)
- **Ejemplo**: `MySecretKey32CharsLong123456` (32 caracteres)

---

## 🚀 Pasos para Configurar en Render

### Opción 1: Desde el Dashboard de Render (Recomendado)

1. **Ve a tu servicio en Render**
   - Accede a: https://dashboard.render.com
   - Selecciona tu servicio: `autonomous-marketing-platform`

2. **Abre la sección "Environment"**
   - En el menú lateral, haz clic en **"Environment"**

3. **Agrega las variables de entorno**
   
   **Para OpenAI API Key:**
   - **Key**: `AI__OpenAI__ApiKey`
   - **Value**: `sk-proj-...` (tu clave completa)
   - ✅ Marca como **"Secret"** (Render lo ocultará)

   **Para Encryption Key:**
   - **Key**: `Encryption__Key`
   - **Value**: `TuClaveDe32CaracteresExactos123` (32 caracteres)
   - ✅ Marca como **"Secret"**

4. **Guarda los cambios**
   - Render reiniciará automáticamente el servicio

### Opción 2: Desde render.yaml (No recomendado para secretos)

⚠️ **NO pongas secretos directamente en `render.yaml`** porque:
- Se sube al repositorio Git
- Cualquiera con acceso al repo puede verlos
- Es una mala práctica de seguridad

El `render.yaml` ya tiene `sync: false` para estos secretos, lo que significa que debes configurarlos manualmente en el dashboard.

---

## 🔍 Verificar que Funciona

### 1. Revisa los logs de Render
Después de configurar los secretos, revisa los logs:
```
https://dashboard.render.com → Tu servicio → Logs
```

Busca mensajes como:
- ✅ `OpenAI API Key configurada correctamente`
- ❌ `OpenAI API Key no configurada. Usando modo mock.`

### 2. Prueba desde la aplicación
- Crea una solicitud de marketing
- Si funciona con IA real → ✅ Secretos configurados correctamente
- Si usa datos mock → ❌ Revisa la configuración

---

## 📝 Notas Importantes

### Formato de Variables de Entorno en .NET
En .NET, las variables de entorno usan `__` (doble guion bajo) para separar niveles:
- `AI__OpenAI__ApiKey` → Se lee como `AI:OpenAI:ApiKey` en el código
- `Encryption__Key` → Se lee como `Encryption:Key` en el código

### Orden de Prioridad
El código busca la API key en este orden:
1. **Base de datos** (configuración por tenant, encriptada) ← **Recomendado para producción**
2. **Variable de entorno** (`AI__OpenAI__ApiKey`) ← **Para configuración global**
3. **Modo Mock** (si no encuentra ninguna)

### Configuración por Tenant (Opcional)
También puedes configurar la API key por tenant desde la aplicación:
- Ve a la sección de configuración de IA en el dashboard
- Cada tenant puede tener su propia API key (se guarda encriptada en la DB)

---

## 🆘 Solución de Problemas

### Error: "OpenAI API Key no configurada"
1. Verifica que la variable esté en Render: `AI__OpenAI__ApiKey`
2. Verifica que el valor no tenga espacios al inicio/final
3. Reinicia el servicio en Render

### Error: "Encryption key must be 32 characters"
1. Verifica que `Encryption__Key` tenga exactamente 32 caracteres
2. Puedes generar una nueva con este comando PowerShell:
   ```powershell
   -join ((65..90) + (97..122) + (48..57) | Get-Random -Count 32 | ForEach-Object {[char]$_})
   ```

### La aplicación usa modo Mock
- Verifica que `AI__UseMock` esté en `"false"` (con comillas)
- Verifica que `AI__OpenAI__ApiKey` tenga un valor válido
- Revisa los logs para ver el error específico

---

## 🔗 Enlaces Útiles

- [Render Environment Variables](https://render.com/docs/environment-variables)
- [OpenAI API Keys](https://platform.openai.com/api-keys)
- [.NET Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)

