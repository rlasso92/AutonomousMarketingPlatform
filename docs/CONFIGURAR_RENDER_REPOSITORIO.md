# 🔧 Configurar Render para Usar el Repositorio Correcto

## 📋 Problema

Render no encuentra el código en `src/` porque el `buildCommand` no especificaba la ruta completa del proyecto.

## ✅ Solución Aplicada

Se actualizó el `render.yaml` para especificar la ruta completa del proyecto:

```yaml
buildCommand: dotnet publish src/AutonomousMarketingPlatform.Web/AutonomousMarketingPlatform.Web.csproj -c Release -o ./publish
```

---

## 🚀 Pasos para Configurar en Render Dashboard

### 1. Conectar el Repositorio

1. **Ve a Render Dashboard**
   - https://dashboard.render.com
   - Si no tienes un servicio creado, haz clic en **"New +"** → **"Web Service"**

2. **Conecta tu Repositorio de GitHub**
   - Selecciona: **"Connect GitHub"** o **"Connect GitLab"**
   - Autoriza Render para acceder a tu repositorio
   - Selecciona: `IrvingCorrosk19/AutonomousMarketingPlatform`

### 2. Configurar el Servicio

Si ya tienes el servicio creado, verifica estas configuraciones:

#### **Basic Settings:**
- **Name**: `autonomous-marketing-platform`
- **Region**: `Virginia (US East)`
- **Branch**: `main` (o `feature/render-deployment` si aún no está en main)
- **Root Directory**: ⚠️ **DEJAR VACÍO** (el proyecto está en la raíz)

#### **Build & Deploy:**
- **Environment**: `Dotnet`
- **Build Command**: 
  ```
  dotnet publish src/AutonomousMarketingPlatform.Web/AutonomousMarketingPlatform.Web.csproj -c Release -o ./publish
  ```
- **Start Command**:
  ```
  cd publish && dotnet AutonomousMarketingPlatform.Web.dll
  ```

### 3. Usar render.yaml (Recomendado)

Si usas `render.yaml` (ya está en tu repositorio), Render lo detectará automáticamente:

1. **En el Dashboard de Render**
   - Ve a tu servicio
   - En la sección **"Settings"** → **"Source"**
   - Verifica que esté usando **"Render Blueprint (render.yaml)"**

2. **Si no lo detecta automáticamente:**
   - Haz clic en **"Manual Deploy"** → **"Deploy latest commit"**
   - Render debería detectar el `render.yaml` y usar esas configuraciones

---

## 🔍 Verificar que Funciona

### 1. Revisa los Logs de Build

Después de hacer push o desplegar manualmente:

1. Ve a tu servicio en Render
2. Haz clic en **"Logs"**
3. Busca en los logs de build:

   ✅ **Si funciona correctamente:**
   ```
   Building...
   Restoring packages...
   Compiling...
   Publishing...
   Build successful
   ```

   ❌ **Si hay errores:**
   ```
   error: The project file 'AutonomousMarketingPlatform.Web.csproj' could not be found
   ```
   → Verifica que el `buildCommand` tenga la ruta correcta: `src/AutonomousMarketingPlatform.Web/...`

### 2. Verifica la Estructura del Proyecto

Tu repositorio debe tener esta estructura:
```
AutonomousMarketingPlatform/
├── render.yaml                    ← Configuración de Render
├── AutonomousMarketingPlatform.sln ← Solution file
├── src/
│   └── AutonomousMarketingPlatform.Web/
│       └── AutonomousMarketingPlatform.Web.csproj  ← Proyecto principal
├── docs/
└── scripts/
```

---

## 📝 Configuración Manual (Si render.yaml no funciona)

Si por alguna razón Render no detecta el `render.yaml`, configura manualmente:

### Build Settings:
```
Build Command: dotnet publish src/AutonomousMarketingPlatform.Web/AutonomousMarketingPlatform.Web.csproj -c Release -o ./publish
```

### Start Settings:
```
Start Command: cd publish && dotnet AutonomousMarketingPlatform.Web.dll
```

### Environment Variables:
Agrega todas las variables que están en `render.yaml`:
- `ASPNETCORE_ENVIRONMENT` = `Production`
- `ASPNETCORE_URLS` = `http://0.0.0.0:$PORT`
- `ConnectionStrings__DefaultConnection` = (tu connection string)
- `AI__OpenAI__ApiKey` = (tu API key - marcada como Secret)
- `Encryption__Key` = (tu clave de 32 caracteres - marcada como Secret)
- etc.

---

## 🆘 Solución de Problemas

### Error: "Project file not found"
- **Causa**: El `buildCommand` no tiene la ruta correcta
- **Solución**: Verifica que sea: `src/AutonomousMarketingPlatform.Web/AutonomousMarketingPlatform.Web.csproj`

### Error: "DLL not found" en Start Command
- **Causa**: El nombre del DLL no coincide
- **Solución**: Verifica que el proyecto se llame `AutonomousMarketingPlatform.Web` (el DLL será `AutonomousMarketingPlatform.Web.dll`)

### Render no detecta el repositorio
- **Causa**: No está conectado o no tiene permisos
- **Solución**: 
  1. Ve a Settings → Source
  2. Haz clic en "Connect GitHub" o "Reconnect"
  3. Autoriza los permisos necesarios

### Build falla con errores de dependencias
- **Causa**: Las dependencias no se restauran correctamente
- **Solución**: Agrega `dotnet restore` antes del build:
  ```
  dotnet restore && dotnet publish src/AutonomousMarketingPlatform.Web/AutonomousMarketingPlatform.Web.csproj -c Release -o ./publish
  ```

---

## 🔗 Enlaces Útiles

- [Render Documentation - .NET](https://render.com/docs/dotnet)
- [Render Blueprint Spec](https://render.com/docs/blueprint-spec)
- [Your Repository](https://github.com/IrvingCorrosk19/AutonomousMarketingPlatform)

---

## ✅ Checklist Final

Antes de desplegar, verifica:

- [ ] El repositorio está conectado en Render
- [ ] La rama `main` (o `feature/render-deployment`) tiene el `render.yaml` actualizado
- [ ] El `buildCommand` apunta a `src/AutonomousMarketingPlatform.Web/...`
- [ ] El `startCommand` usa `AutonomousMarketingPlatform.Web.dll`
- [ ] Las variables de entorno están configuradas (especialmente los secretos)
- [ ] El servicio está en la región correcta (Virginia)

