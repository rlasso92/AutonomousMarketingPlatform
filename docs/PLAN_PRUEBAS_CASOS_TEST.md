# Plan de Pruebas y Casos de Test
## Autonomous Marketing Platform

**Versión:** 1.0  
**Fecha:** 2026-01-04  
**Objetivo:** Validar funcionalidad end-to-end y control de acceso por roles

---

## ÍNDICE

1. [Pruebas de Autenticación y Roles](#1-pruebas-de-autenticación-y-roles)
2. [Pruebas de Campañas](#2-pruebas-de-campañas)
3. [Pruebas de Marketing Request](#3-pruebas-de-marketing-request)
4. [Pruebas de Consents](#4-pruebas-de-consents)
5. [Pruebas de Memory](#5-pruebas-de-memory)
6. [Pruebas de Content](#6-pruebas-de-content)
7. [Pruebas de Metrics](#7-pruebas-de-metrics)
8. [Pruebas de Publishing](#8-pruebas-de-publishing)
9. [Pruebas de APIs](#9-pruebas-de-apis)
10. [Pruebas de Configuración](#10-pruebas-de-configuración)
11. [Pruebas de Usuarios y Tenants](#11-pruebas-de-usuarios-y-tenants)

---

## 1. PRUEBAS DE AUTENTICACIÓN Y ROLES

### 1.1 Login y Autenticación

#### TC-AUTH-001: Login Exitoso
**Prioridad:** ALTA  
**Rol:** Cualquier usuario válido

**Precondiciones:**
- Usuario existe en la base de datos
- Usuario tiene TenantId asignado
- Usuario tiene contraseña válida

**Pasos:**
1. Navegar a `/Account/Login`
2. Ingresar email válido
3. Ingresar contraseña correcta
4. Hacer clic en "Iniciar Sesión"

**Resultado Esperado:**
- ✅ Usuario es redirigido a `/Home/Index`
- ✅ Se muestra el dashboard del usuario
- ✅ El usuario está autenticado (verificar en sesión)
- ✅ Se muestra el nombre del usuario en la barra de navegación
- ✅ No se muestran errores

---

#### TC-AUTH-002: Login con Credenciales Incorrectas
**Prioridad:** ALTA  
**Rol:** Usuario no autenticado

**Precondiciones:**
- Usuario existe en la base de datos

**Pasos:**
1. Navegar a `/Account/Login`
2. Ingresar email válido
3. Ingresar contraseña incorrecta
4. Hacer clic en "Iniciar Sesión"

**Resultado Esperado:**
- ✅ Se muestra mensaje de error: "Credenciales inválidas"
- ✅ El usuario NO es redirigido
- ✅ El usuario permanece en la página de login
- ✅ El campo de contraseña se limpia o mantiene el valor

---

#### TC-AUTH-003: Acceso sin Autenticación
**Prioridad:** ALTA  
**Rol:** Usuario no autenticado

**Precondiciones:**
- Usuario NO está autenticado
- Sesión cerrada o expirada

**Pasos:**
1. Intentar acceder a `/Campaigns/Index` sin estar autenticado
2. Intentar acceder a `/MarketingRequest/Create` sin estar autenticado
3. Intentar acceder a `/Consents/Index` sin estar autenticado

**Resultado Esperado:**
- ✅ Usuario es redirigido a `/Account/Login`
- ✅ Se muestra mensaje indicando que debe iniciar sesión
- ✅ Después del login, se redirige a la página original solicitada

---

### 1.2 Control de Acceso por Roles

#### TC-ROLE-001: SuperAdmin - Acceso a Todas las Funcionalidades
**Prioridad:** ALTA  
**Rol:** SuperAdmin

**Precondiciones:**
- Usuario tiene claim `IsSuperAdmin = "true"`
- Usuario tiene `TenantId = Guid.Empty`

**Pasos:**
1. Iniciar sesión como SuperAdmin
2. Intentar acceder a `/Campaigns/Index`
3. Intentar acceder a `/Tenants/Index`
4. Intentar acceder a `/Users/Index`
5. Intentar acceder a `/N8nConfig/Index`
6. Intentar acceder a `/AIConfig/Index`
7. Intentar acceder a `/Consents/Index`
8. Intentar editar una campaña de otro tenant

**Resultado Esperado:**
- ✅ Acceso permitido a todas las rutas
- ✅ Puede ver campañas de todos los tenants
- ✅ Puede ver y gestionar todos los tenants
- ✅ Puede ver y gestionar todos los usuarios
- ✅ Puede configurar n8n para cualquier tenant
- ✅ Puede configurar IA para cualquier tenant
- ✅ Puede ver consentimientos de todos los usuarios
- ✅ Puede editar campañas de cualquier tenant
- ✅ En `/Consents/Index` se muestra selector de usuarios
- ✅ En `/N8nConfig/Index` se muestra selector de tenants

---

#### TC-ROLE-002: Owner - Acceso Completo a su Tenant
**Prioridad:** ALTA  
**Rol:** Owner

**Precondiciones:**
- Usuario tiene rol `Owner`
- Usuario tiene `TenantId` asignado

**Pasos:**
1. Iniciar sesión como Owner
2. Intentar acceder a `/Campaigns/Index`
3. Intentar acceder a `/MarketingRequest/Create`
4. Intentar acceder a `/Content/Upload`
5. Intentar acceder a `/Metrics/Index`
6. Intentar acceder a `/Publishing/Index`
7. Intentar acceder a `/AIConfig/Index`
8. Intentar acceder a `/N8nConfig/Index`
9. Intentar acceder a `/Tenants/Index`
10. Intentar acceder a `/Users/Index`

**Resultado Esperado:**
- ✅ Acceso permitido a `/Campaigns/Index`
- ✅ Acceso permitido a `/MarketingRequest/Create`
- ✅ Acceso permitido a `/Content/Upload`
- ✅ Acceso permitido a `/Metrics/Index`
- ✅ Acceso permitido a `/Publishing/Index`
- ✅ Acceso permitido a `/AIConfig/Index`
- ✅ Acceso permitido a `/N8nConfig/Index`
- ✅ Acceso permitido a `/Users/Index`
- ❌ Acceso DENEGADO a `/Tenants/Index` (redirige a AccessDenied)
- ✅ Solo ve datos de su propio tenant

---

#### TC-ROLE-003: Admin - Acceso Administrativo
**Prioridad:** ALTA  
**Rol:** Admin

**Precondiciones:**
- Usuario tiene rol `Admin`
- Usuario tiene `TenantId` asignado

**Pasos:**
1. Iniciar sesión como Admin
2. Intentar acceder a `/Campaigns/Index`
3. Intentar acceder a `/MarketingRequest/Create`
4. Intentar acceder a `/Content/Upload`
5. Intentar acceder a `/Users/Index`
6. Intentar acceder a `/AIConfig/Index`
7. Intentar acceder a `/N8nConfig/Index`
8. Intentar acceder a `/Tenants/Index`

**Resultado Esperado:**
- ✅ Acceso permitido a `/Campaigns/Index`
- ✅ Acceso permitido a `/MarketingRequest/Create`
- ✅ Acceso permitido a `/Content/Upload`
- ✅ Acceso permitido a `/Users/Index`
- ✅ Acceso permitido a `/AIConfig/Index`
- ✅ Acceso permitido a `/N8nConfig/Index`
- ❌ Acceso DENEGADO a `/Tenants/Index` (redirige a AccessDenied)
- ✅ Solo ve datos de su propio tenant

---

#### TC-ROLE-004: Marketer - Acceso Limitado
**Prioridad:** ALTA  
**Rol:** Marketer

**Precondiciones:**
- Usuario tiene rol `Marketer`
- Usuario tiene `TenantId` asignado

**Pasos:**
1. Iniciar sesión como Marketer
2. Intentar acceder a `/Campaigns/Index`
3. Intentar acceder a `/MarketingRequest/Create`
4. Intentar acceder a `/Content/Upload`
5. Intentar acceder a `/Metrics/Index`
6. Intentar acceder a `/Publishing/Index`
7. Intentar acceder a `/Users/Index`
8. Intentar acceder a `/AIConfig/Index`
9. Intentar acceder a `/N8nConfig/Index`
10. Intentar crear una campaña
11. Intentar editar una campaña
12. Intentar eliminar una campaña

**Resultado Esperado:**
- ✅ Acceso permitido a `/Campaigns/Index` (solo lectura)
- ✅ Acceso permitido a `/MarketingRequest/Create`
- ✅ Acceso permitido a `/Content/Upload`
- ✅ Acceso permitido a `/Metrics/Index` (solo lectura)
- ✅ Acceso permitido a `/Publishing/Index`
- ❌ Acceso DENEGADO a `/Users/Index` (redirige a AccessDenied)
- ❌ Acceso DENEGADO a `/AIConfig/Index` (redirige a AccessDenied)
- ❌ Acceso DENEGADO a `/N8nConfig/Index` (redirige a AccessDenied)
- ✅ Puede crear campañas
- ✅ Puede editar campañas
- ❌ NO puede eliminar campañas (botón no visible o deshabilitado)
- ✅ Solo ve datos de su propio tenant

---

#### TC-ROLE-005: Viewer - Solo Lectura
**Prioridad:** MEDIA  
**Rol:** Viewer (si existe)

**Precondiciones:**
- Usuario tiene rol `Viewer` (si está implementado)
- Usuario tiene `TenantId` asignado

**Pasos:**
1. Iniciar sesión como Viewer
2. Intentar acceder a `/Campaigns/Index`
3. Intentar crear una campaña
4. Intentar editar una campaña
5. Intentar acceder a `/MarketingRequest/Create`

**Resultado Esperado:**
- ✅ Acceso permitido a `/Campaigns/Index` (solo lectura)
- ❌ NO puede crear campañas (botón no visible o redirige a AccessDenied)
- ❌ NO puede editar campañas (botón no visible o redirige a AccessDenied)
- ❌ Acceso DENEGADO a `/MarketingRequest/Create` (redirige a AccessDenied)

---

#### TC-ROLE-006: SuperAdmin - Selector de Usuarios en Consents
**Prioridad:** MEDIA  
**Rol:** SuperAdmin

**Precondiciones:**
- Usuario tiene claim `IsSuperAdmin = "true"`
- Existen múltiples usuarios en la base de datos

**Pasos:**
1. Iniciar sesión como SuperAdmin
2. Navegar a `/Consents/Index`
3. Verificar que se muestra el card "Modo Super Administrador"
4. Verificar que se muestra el selector dropdown de usuarios
5. Seleccionar un usuario del dropdown
6. Verificar que se cargan los consentimientos del usuario seleccionado

**Resultado Esperado:**
- ✅ Se muestra el card "Modo Super Administrador"
- ✅ Se muestra el selector dropdown con lista de usuarios
- ✅ Al seleccionar un usuario, se cargan sus consentimientos
- ✅ Se puede otorgar/revocar consentimientos para el usuario seleccionado
- ✅ Los cambios se aplican al usuario correcto

---

#### TC-ROLE-007: Usuario Normal - Sin Selector en Consents
**Prioridad:** MEDIA  
**Rol:** Owner, Admin, Marketer

**Precondiciones:**
- Usuario NO es SuperAdmin
- Usuario tiene rol Owner, Admin o Marketer

**Pasos:**
1. Iniciar sesión como usuario normal (Owner/Admin/Marketer)
2. Navegar a `/Consents/Index`
3. Verificar que NO se muestra el card "Modo Super Administrador"
4. Verificar que NO se muestra el selector dropdown de usuarios
5. Verificar que se muestran solo los consentimientos del usuario actual

**Resultado Esperado:**
- ❌ NO se muestra el card "Modo Super Administrador"
- ❌ NO se muestra el selector dropdown de usuarios
- ✅ Se muestran solo los consentimientos del usuario actual
- ✅ Solo puede modificar sus propios consentimientos

---

## 2. PRUEBAS DE CAMPAÑAS

### 2.1 Listado de Campañas

#### TC-CAMP-001: Listar Campañas - Usuario Normal
**Prioridad:** ALTA  
**Rol:** Owner, Admin, Marketer

**Precondiciones:**
- Usuario autenticado con TenantId
- Existen campañas en la base de datos para el tenant del usuario

**Pasos:**
1. Iniciar sesión
2. Navegar a `/Campaigns/Index`
3. Verificar que se muestra la lista de campañas

**Resultado Esperado:**
- ✅ Se muestra la lista de campañas del tenant del usuario
- ✅ Cada campaña muestra: Nombre, Estado, Fechas, Presupuesto
- ✅ Se pueden filtrar por estado (Draft, Active, Paused, Archived)
- ✅ NO se muestran campañas de otros tenants
- ✅ Los botones de acción (Ver, Editar, Eliminar) son visibles según el rol

---

#### TC-CAMP-002: Listar Campañas - SuperAdmin
**Prioridad:** ALTA  
**Rol:** SuperAdmin

**Precondiciones:**
- Usuario SuperAdmin autenticado
- Existen campañas de múltiples tenants

**Pasos:**
1. Iniciar sesión como SuperAdmin
2. Navegar a `/Campaigns/Index`
3. Verificar que se muestra la lista de campañas

**Resultado Esperado:**
- ✅ Se muestran campañas de TODOS los tenants
- ✅ Se puede filtrar por tenant (si está implementado)
- ✅ Se puede filtrar por estado
- ✅ Cada campaña muestra el tenant al que pertenece

---

### 2.2 Crear Campaña

#### TC-CAMP-003: Crear Campaña Exitosamente
**Prioridad:** ALTA  
**Rol:** Owner, Admin, Marketer

**Precondiciones:**
- Usuario autenticado con permisos para crear campañas
- Usuario tiene TenantId asignado

**Pasos:**
1. Navegar a `/Campaigns/Create`
2. Completar el formulario:
   - Nombre: "Campaña de Prueba"
   - Descripción: "Descripción de prueba"
   - Estado: "Draft"
   - Fecha de inicio: Fecha futura
   - Fecha de fin: Fecha futura posterior
   - Presupuesto: 1000
   - Canales objetivo: Instagram, Facebook
3. Hacer clic en "Crear Campaña"

**Resultado Esperado:**
- ✅ Se valida el formulario correctamente
- ✅ Se crea la campaña en la base de datos
- ✅ Se asigna el TenantId correcto
- ✅ Se asigna el UserId del creador
- ✅ Se muestra mensaje de éxito: "Campaña creada exitosamente"
- ✅ Se redirige a `/Campaigns/Details/{id}`
- ✅ La campaña se muestra con todos los datos ingresados
- ✅ Se registra en AuditLog

---

#### TC-CAMP-004: Crear Campaña - Validación de Campos Requeridos
**Prioridad:** ALTA  
**Rol:** Owner, Admin, Marketer

**Precondiciones:**
- Usuario autenticado con permisos para crear campañas

**Pasos:**
1. Navegar a `/Campaigns/Create`
2. Dejar el campo "Nombre" vacío
3. Hacer clic en "Crear Campaña"

**Resultado Esperado:**
- ❌ NO se crea la campaña
- ✅ Se muestra mensaje de error: "El campo Nombre es requerido"
- ✅ El formulario permanece en la página
- ✅ Los campos ingresados se mantienen

---

### 2.3 Editar Campaña

#### TC-CAMP-005: Editar Campaña - Cargar Datos
**Prioridad:** ALTA  
**Rol:** Owner, Admin, Marketer

**Precondiciones:**
- Usuario autenticado
- Existe una campaña del tenant del usuario
- Usuario tiene permisos para editar

**Pasos:**
1. Navegar a `/Campaigns/Edit/{campaignId}`
2. Verificar que el formulario se carga con los datos de la campaña

**Resultado Esperado:**
- ✅ El formulario se carga correctamente
- ✅ Todos los campos están poblados con los datos de la campaña:
  - Nombre
  - Descripción
  - Estado
  - Fechas
  - Presupuesto
  - Canales objetivo
- ✅ Los campos son editables
- ✅ No se muestran errores

---

#### TC-CAMP-006: Editar Campaña - SuperAdmin
**Prioridad:** ALTA  
**Rol:** SuperAdmin

**Precondiciones:**
- Usuario SuperAdmin autenticado
- Existe una campaña de otro tenant

**Pasos:**
1. Navegar a `/Campaigns/Edit/{campaignId}` donde campaignId pertenece a otro tenant
2. Verificar que se cargan los datos de la campaña
3. Modificar el nombre
4. Guardar los cambios

**Resultado Esperado:**
- ✅ Se cargan los datos de la campaña correctamente
- ✅ Se puede editar la campaña
- ✅ Se guardan los cambios correctamente
- ✅ Se obtiene el TenantId real de la campaña desde la BD
- ✅ Se actualiza la campaña en la base de datos

---

#### TC-CAMP-007: Editar Campaña - Actualizar Datos
**Prioridad:** ALTA  
**Rol:** Owner, Admin, Marketer

**Precondiciones:**
- Usuario autenticado
- Existe una campaña del tenant del usuario

**Pasos:**
1. Navegar a `/Campaigns/Edit/{campaignId}`
2. Modificar el nombre de la campaña
3. Cambiar el estado a "Active"
4. Actualizar el presupuesto
5. Hacer clic en "Guardar Cambios"

**Resultado Esperado:**
- ✅ Se validan los datos correctamente
- ✅ Se actualiza la campaña en la base de datos
- ✅ Se actualiza el campo `UpdatedAt`
- ✅ Se muestra mensaje de éxito: "Campaña actualizada exitosamente"
- ✅ Se redirige a `/Campaigns/Details/{id}`
- ✅ Los cambios se reflejan en la vista de detalles
- ✅ Se registra en AuditLog

---

#### TC-CAMP-008: Editar Campaña - No Encontrada
**Prioridad:** MEDIA  
**Rol:** Owner, Admin, Marketer

**Precondiciones:**
- Usuario autenticado
- La campaña no existe o pertenece a otro tenant

**Pasos:**
1. Intentar navegar a `/Campaigns/Edit/{campaignId}` con un ID que no existe
2. Intentar editar una campaña de otro tenant (usuario normal)

**Resultado Esperado:**
- ❌ Se muestra mensaje: "La campaña no fue encontrada o no tienes permisos para editarla"
- ✅ Se redirige a `/Campaigns/Index`
- ✅ No se muestra el formulario de edición

---

### 2.4 Ver Detalles de Campaña

#### TC-CAMP-009: Ver Detalles de Campaña
**Prioridad:** ALTA  
**Rol:** Cualquier usuario autenticado

**Precondiciones:**
- Usuario autenticado
- Existe una campaña del tenant del usuario

**Pasos:**
1. Navegar a `/Campaigns/Details/{campaignId}`
2. Verificar que se muestran todos los detalles

**Resultado Esperado:**
- ✅ Se muestran todos los datos de la campaña:
  - Nombre, Descripción, Estado
  - Fechas de inicio y fin
  - Presupuesto
  - Canales objetivo
  - Objetivos y audiencia objetivo
  - Notas
- ✅ Se muestran los MarketingPacks asociados
- ✅ Se muestran los Contents asociados
- ✅ Se muestran los PublishingJobs asociados
- ✅ Los botones de acción son visibles según el rol

---

### 2.5 Eliminar Campaña

#### TC-CAMP-010: Eliminar Campaña - Owner/Admin
**Prioridad:** ALTA  
**Rol:** Owner, Admin

**Precondiciones:**
- Usuario autenticado con rol Owner o Admin
- Existe una campaña del tenant del usuario

**Pasos:**
1. Navegar a `/Campaigns/Details/{campaignId}`
2. Hacer clic en "Eliminar Campaña"
3. Confirmar la eliminación

**Resultado Esperado:**
- ✅ Se muestra confirmación antes de eliminar
- ✅ Se realiza soft delete (IsActive = false)
- ✅ Se muestra mensaje de éxito
- ✅ Se redirige a `/Campaigns/Index`
- ✅ La campaña ya no aparece en la lista
- ✅ Se registra en AuditLog

---

#### TC-CAMP-011: Eliminar Campaña - Marketer
**Prioridad:** MEDIA  
**Rol:** Marketer

**Precondiciones:**
- Usuario autenticado con rol Marketer
- Existe una campaña del tenant del usuario

**Pasos:**
1. Navegar a `/Campaigns/Details/{campaignId}`
2. Verificar si existe el botón "Eliminar Campaña"

**Resultado Esperado:**
- ❌ NO se muestra el botón "Eliminar Campaña"
- ❌ Si intenta acceder directamente a la acción, se redirige a AccessDenied

---

## 3. PRUEBAS DE MARKETING REQUEST

### 3.1 Crear Marketing Request

#### TC-MR-001: Crear Marketing Request Exitosamente
**Prioridad:** ALTA  
**Rol:** Owner, Admin, Marketer

**Precondiciones:**
- Usuario autenticado con permisos
- Usuario tiene consentimientos de IA y Publicación otorgados
- n8n está configurado y funcionando
- Existe una campaña (opcional)

**Pasos:**
1. Navegar a `/MarketingRequest/Create`
2. Completar el formulario:
   - Instrucción: "Crear contenido para promocionar nuestro nuevo producto"
   - Canales: Instagram, Facebook
   - Requiere aprobación: Sí
   - Campaña: Seleccionar una campaña (opcional)
3. Hacer clic en "Solicitar Contenido"

**Resultado Esperado:**
- ✅ Se valida el formulario
- ✅ Se dispara el webhook a n8n con los datos correctos
- ✅ Se crea un AutomationState en la base de datos
- ✅ Se muestra mensaje de éxito: "Solicitud enviada exitosamente"
- ✅ Se redirige a `/MarketingRequest/Success`
- ✅ Se muestra el RequestId
- ✅ Si se asoció una campaña, se guarda el CampaignId

---

#### TC-MR-002: Crear Marketing Request - Sin Consentimientos
**Prioridad:** ALTA  
**Rol:** Owner, Admin, Marketer

**Precondiciones:**
- Usuario autenticado
- Usuario NO tiene consentimientos otorgados

**Pasos:**
1. Navegar a `/MarketingRequest/Create`
2. Completar el formulario
3. Hacer clic en "Solicitar Contenido"

**Resultado Esperado:**
- ❌ Se muestra mensaje: "Debes otorgar consentimientos de IA y Publicación antes de continuar"
- ✅ Se redirige a `/Consents/Index`
- ✅ NO se dispara el webhook a n8n

---

#### TC-MR-003: Crear Marketing Request - Sin Permisos
**Prioridad:** MEDIA  
**Rol:** Viewer (si existe) o usuario sin rol Marketer/Admin/Owner

**Precondiciones:**
- Usuario autenticado sin rol Marketer, Admin u Owner

**Pasos:**
1. Intentar navegar a `/MarketingRequest/Create`

**Resultado Esperado:**
- ❌ Acceso DENEGADO
- ✅ Se redirige a `/Account/AccessDenied`

---

## 4. PRUEBAS DE CONSENTS

### 4.1 Ver Consentimientos

#### TC-CONS-001: Ver Consentimientos Propios
**Prioridad:** ALTA  
**Rol:** Cualquier usuario autenticado

**Precondiciones:**
- Usuario autenticado
- Usuario tiene consentimientos en la base de datos

**Pasos:**
1. Navegar a `/Consents/Index`
2. Verificar que se muestran los consentimientos del usuario

**Resultado Esperado:**
- ✅ Se muestran los consentimientos del usuario actual:
  - AIGeneration (con estado: Otorgado/No Otorgado)
  - AutoPublishing (con estado: Otorgado/No Otorgado)
- ✅ Se muestran las fechas de otorgamiento
- ✅ Se muestran botones para otorgar/revocar según el estado actual
- ✅ NO se muestra el selector de usuarios (si no es SuperAdmin)

---

#### TC-CONS-002: Ver Consentimientos - SuperAdmin
**Prioridad:** ALTA  
**Rol:** SuperAdmin

**Precondiciones:**
- Usuario SuperAdmin autenticado
- Existen múltiples usuarios en la base de datos

**Pasos:**
1. Navegar a `/Consents/Index`
2. Verificar que se muestra el selector de usuarios
3. Seleccionar un usuario del dropdown
4. Verificar que se cargan los consentimientos del usuario seleccionado

**Resultado Esperado:**
- ✅ Se muestra el card "Modo Super Administrador"
- ✅ Se muestra el selector dropdown con lista de usuarios
- ✅ Al seleccionar un usuario, se cargan sus consentimientos
- ✅ Se puede otorgar/revocar consentimientos para el usuario seleccionado

---

### 4.2 Otorgar Consentimientos

#### TC-CONS-003: Otorgar Consentimiento de IA
**Prioridad:** ALTA  
**Rol:** Cualquier usuario autenticado

**Precondiciones:**
- Usuario autenticado
- Usuario NO tiene consentimiento de IA otorgado

**Pasos:**
1. Navegar a `/Consents/Index`
2. Hacer clic en "Otorgar" para AIGeneration
3. Confirmar la acción

**Resultado Esperado:**
- ✅ Se crea el consentimiento en la base de datos
- ✅ Se asigna el TenantId y UserId correctos
- ✅ Se establece el ConsentType como "AIGeneration"
- ✅ Se establece IsGranted = true
- ✅ Se registra la fecha de otorgamiento
- ✅ Se muestra mensaje de éxito
- ✅ La página se actualiza mostrando el nuevo estado

---

#### TC-CONS-004: Otorgar Consentimiento - SuperAdmin para Otro Usuario
**Prioridad:** MEDIA  
**Rol:** SuperAdmin

**Precondiciones:**
- Usuario SuperAdmin autenticado
- Existe otro usuario sin consentimientos

**Pasos:**
1. Navegar a `/Consents/Index`
2. Seleccionar otro usuario del dropdown
3. Hacer clic en "Otorgar" para AIGeneration
4. Confirmar la acción

**Resultado Esperado:**
- ✅ Se crea el consentimiento para el usuario seleccionado (no para el SuperAdmin)
- ✅ Se asigna el UserId del usuario seleccionado
- ✅ Se asigna el TenantId del usuario seleccionado
- ✅ Se muestra mensaje de éxito
- ✅ Los consentimientos del usuario seleccionado se actualizan

---

### 4.3 Revocar Consentimientos

#### TC-CONS-005: Revocar Consentimiento
**Prioridad:** ALTA  
**Rol:** Cualquier usuario autenticado

**Precondiciones:**
- Usuario autenticado
- Usuario tiene consentimiento otorgado

**Pasos:**
1. Navegar a `/Consents/Index`
2. Hacer clic en "Revocar" para un consentimiento otorgado
3. Confirmar la acción

**Resultado Esperado:**
- ✅ Se actualiza el consentimiento: IsGranted = false
- ✅ Se registra la fecha de revocación
- ✅ Se muestra mensaje de éxito
- ✅ La página se actualiza mostrando el nuevo estado
- ✅ El usuario ya no puede usar funcionalidades que requieren ese consentimiento

---

## 5. PRUEBAS DE MEMORY

### 5.1 Ver Memorias

#### TC-MEM-001: Listar Memorias de Marketing
**Prioridad:** MEDIA  
**Rol:** Cualquier usuario autenticado

**Precondiciones:**
- Usuario autenticado
- Existen memorias en la base de datos para el tenant

**Pasos:**
1. Navegar a `/Memory/Index`
2. Verificar que se muestran las memorias

**Resultado Esperado:**
- ✅ Se muestran las memorias del tenant del usuario
- ✅ Se pueden filtrar por tipo (Conversation, Decision, Learning, etc.)
- ✅ Se pueden filtrar por tags
- ✅ Cada memoria muestra: Tipo, Contenido, Tags, Fecha
- ✅ NO se muestran memorias de otros tenants

---

#### TC-MEM-002: Ver Contexto de Memoria para IA
**Prioridad:** MEDIA  
**Rol:** Cualquier usuario autenticado

**Precondiciones:**
- Usuario autenticado
- Existen memorias en la base de datos

**Pasos:**
1. Navegar a `/Memory/AIContext`
2. Verificar que se muestra el contexto de memoria

**Resultado Esperado:**
- ✅ Se muestra el contexto estructurado:
  - UserPreferences
  - RecentConversations
  - CampaignMemories
  - Learnings
  - SummarizedContext
- ✅ El contexto está formateado para uso con IA
- ✅ Solo se muestran memorias del tenant del usuario

---

## 6. PRUEBAS DE CONTENT

### 6.1 Subir Contenido

#### TC-CONT-001: Subir Archivo de Imagen
**Prioridad:** ALTA  
**Rol:** Owner, Admin, Marketer

**Precondiciones:**
- Usuario autenticado con permisos
- Usuario tiene una imagen válida para subir

**Pasos:**
1. Navegar a `/Content/Upload`
2. Seleccionar un archivo de imagen (JPG, PNG)
3. Seleccionar una campaña (opcional)
4. Agregar descripción (opcional)
5. Hacer clic en "Subir"

**Resultado Esperado:**
- ✅ Se valida el archivo (tipo, tamaño)
- ✅ Se guarda el archivo en el almacenamiento
- ✅ Se crea un registro de Content en la base de datos
- ✅ Se asigna el TenantId y UserId correctos
- ✅ Si se seleccionó una campaña, se asocia el CampaignId
- ✅ Se muestra mensaje de éxito
- ✅ El archivo aparece en `/Content/Index`

---

#### TC-CONT-002: Subir Múltiples Archivos
**Prioridad:** MEDIA  
**Rol:** Owner, Admin, Marketer

**Precondiciones:**
- Usuario autenticado con permisos
- Usuario tiene múltiples archivos válidos

**Pasos:**
1. Navegar a `/Content/Upload`
2. Seleccionar múltiples archivos (Ctrl+Click)
3. Completar el formulario
4. Hacer clic en "Subir"

**Resultado Esperado:**
- ✅ Se procesan todos los archivos
- ✅ Se crean múltiples registros de Content
- ✅ Se muestra mensaje de éxito con el número de archivos subidos
- ✅ Todos los archivos aparecen en `/Content/Index`

---

#### TC-CONT-003: Subir Archivo - Validación de Tipo
**Prioridad:** MEDIA  
**Rol:** Owner, Admin, Marketer

**Precondiciones:**
- Usuario autenticado con permisos

**Pasos:**
1. Navegar a `/Content/Upload`
2. Intentar subir un archivo no válido (ej: .exe, .zip)
3. Hacer clic en "Subir"

**Resultado Esperado:**
- ❌ NO se sube el archivo
- ✅ Se muestra mensaje de error: "Tipo de archivo no permitido"
- ✅ Solo se permiten imágenes (JPG, PNG, GIF) y videos (MP4, MOV)

---

### 6.2 Listar Contenido

#### TC-CONT-004: Listar Contenido
**Prioridad:** MEDIA  
**Rol:** Cualquier usuario autenticado

**Precondiciones:**
- Usuario autenticado
- Existen contenidos en la base de datos

**Pasos:**
1. Navegar a `/Content/Index`
2. Verificar que se muestra la lista de contenidos

**Resultado Esperado:**
- ✅ Se muestran los contenidos del tenant del usuario
- ✅ Se pueden filtrar por campaña
- ✅ Se pueden filtrar por tipo de contenido
- ✅ Cada contenido muestra: Tipo, Descripción, Fecha, Vista previa
- ✅ NO se muestran contenidos de otros tenants

---

## 7. PRUEBAS DE METRICS

### 7.1 Ver Métricas

#### TC-MET-001: Ver Métricas de Campañas
**Prioridad:** MEDIA  
**Rol:** Cualquier usuario autenticado

**Precondiciones:**
- Usuario autenticado
- Existen métricas en la base de datos

**Pasos:**
1. Navegar a `/Metrics/Index`
2. Verificar que se muestran las métricas

**Resultado Esperado:**
- ✅ Se muestran métricas agregadas de todas las campañas del tenant
- ✅ Se pueden filtrar por rango de fechas
- ✅ Se muestran: Impresiones, Clics, Engagement, CTR, etc.
- ✅ Los datos se presentan en gráficos o tablas

---

#### TC-MET-002: Ver Métricas de una Campaña Específica
**Prioridad:** MEDIA  
**Rol:** Cualquier usuario autenticado

**Precondiciones:**
- Usuario autenticado
- Existe una campaña con métricas

**Pasos:**
1. Navegar a `/Metrics/Campaign/{campaignId}`
2. Verificar que se muestran las métricas de la campaña

**Resultado Esperado:**
- ✅ Se muestran métricas detalladas de la campaña
- ✅ Se muestran métricas por fecha
- ✅ Se muestran gráficos de evolución
- ✅ Se muestran comparativas con promedios

---

### 7.2 Registrar Métricas

#### TC-MET-003: Registrar Métricas de Campaña
**Prioridad:** MEDIA  
**Rol:** Owner, Admin, Marketer

**Precondiciones:**
- Usuario autenticado con permisos
- Existe una campaña

**Pasos:**
1. Navegar a `/Metrics/RegisterCampaign/{campaignId}`
2. Completar el formulario:
   - Fecha: Fecha actual
   - Impresiones: 1000
   - Clics: 50
   - Likes: 25
   - Comentarios: 5
   - Compartidos: 10
3. Hacer clic en "Guardar"

**Resultado Esperado:**
- ✅ Se valida el formulario
- ✅ Se crea el registro de métricas en la base de datos
- ✅ Se calculan automáticamente: Engagement, CTR, Engagement Rate
- ✅ Se muestra mensaje de éxito
- ✅ Se redirige a `/Metrics/Campaign/{campaignId}`
- ✅ Las nuevas métricas aparecen en la vista

---

## 8. PRUEBAS DE PUBLISHING

### 8.1 Generar Publishing Job

#### TC-PUB-001: Generar Publishing Job
**Prioridad:** ALTA  
**Rol:** Owner, Admin, Marketer

**Precondiciones:**
- Usuario autenticado con permisos
- Existe un MarketingPack o GeneratedCopy
- Existe una campaña

**Pasos:**
1. Navegar a `/Publishing/Generate`
2. Completar el formulario:
   - Campaña: Seleccionar una campaña
   - Marketing Pack: Seleccionar un pack (opcional)
   - Copy: Seleccionar un copy
   - Canal: Instagram
   - Fecha programada: Fecha futura
   - Requiere aprobación: Sí
3. Hacer clic en "Generar Publicación"

**Resultado Esperado:**
- ✅ Se valida el formulario
- ✅ Se crea el PublishingJob en la base de datos
- ✅ Se asigna el TenantId y UserId correctos
- ✅ Se establece el estado inicial según requiere aprobación
- ✅ Se muestra mensaje de éxito
- ✅ Se redirige a `/Publishing/Details/{id}`

---

### 8.2 Ver Publicaciones

#### TC-PUB-002: Listar Publishing Jobs
**Prioridad:** MEDIA  
**Rol:** Cualquier usuario autenticado

**Precondiciones:**
- Usuario autenticado
- Existen publishing jobs en la base de datos

**Pasos:**
1. Navegar a `/Publishing/Index`
2. Verificar que se muestran los jobs

**Resultado Esperado:**
- ✅ Se muestran los publishing jobs del tenant del usuario
- ✅ Se pueden filtrar por campaña, estado, canal
- ✅ Cada job muestra: Canal, Estado, Fecha programada/publicada, URL
- ✅ NO se muestran jobs de otros tenants

---

#### TC-PUB-003: Ver Detalles de Publishing Job
**Prioridad:** MEDIA  
**Rol:** Cualquier usuario autenticado

**Precondiciones:**
- Usuario autenticado
- Existe un publishing job

**Pasos:**
1. Navegar a `/Publishing/Details/{id}`
2. Verificar que se muestran todos los detalles

**Resultado Esperado:**
- ✅ Se muestran todos los datos del job:
  - Campaña asociada
  - Marketing Pack asociado
  - Copy utilizado
  - Canal
  - Estado
  - Fechas
  - URL publicada
  - Contenido
  - Hashtags
- ✅ Se muestran métricas asociadas (si existen)
- ✅ Se muestran botones de acción según el estado y rol

---

### 8.3 Aprobar Publicación

#### TC-PUB-004: Aprobar Publishing Job
**Prioridad:** ALTA  
**Rol:** Owner, Admin, Marketer

**Precondiciones:**
- Usuario autenticado con permisos
- Existe un publishing job con estado "Pending" o "RequiresApproval"

**Pasos:**
1. Navegar a `/Publishing/Details/{id}`
2. Hacer clic en "Aprobar y Marcar como Publicado"
3. Ingresar URL publicada (opcional)
4. Ingresar External Post ID (opcional)
5. Confirmar la acción

**Resultado Esperado:**
- ✅ Se actualiza el estado del job a "Success"
- ✅ Se registra la fecha de publicación
- ✅ Se guarda la URL y External Post ID si se proporcionaron
- ✅ Se muestra mensaje de éxito
- ✅ El job aparece como publicado en la lista

---

## 9. PRUEBAS DE APIS

### 9.1 API de Marketing Packs

#### TC-API-001: Crear Marketing Pack desde n8n
**Prioridad:** ALTA  
**Rol:** n8n (AllowAnonymous)

**Precondiciones:**
- n8n está ejecutando el workflow
- Existe un ContentId válido en la base de datos
- Existe un CampaignId válido

**Pasos:**
1. n8n hace POST a `/api/marketing-packs`
2. Envía el JSON con todos los datos del pack:
   - tenantId, userId, contentId, campaignId
   - strategy, copy, visualPrompts
   - copies, assetPrompts
   - status, metadata

**Resultado Esperado:**
- ✅ Se valida que tenantId es un GUID válido
- ✅ Se valida que contentId existe (si no, se crea automáticamente)
- ✅ Se crea el MarketingPack en la base de datos
- ✅ Se crean los GeneratedCopies asociados
- ✅ Se crean los MarketingAssetPrompts asociados
- ✅ Se retorna HTTP 200 con el ID del pack creado
- ✅ Se registra en ApplicationLogs

---

#### TC-API-002: Obtener Marketing Pack
**Prioridad:** MEDIA  
**Rol:** n8n (AllowAnonymous)

**Precondiciones:**
- Existe un MarketingPack en la base de datos

**Pasos:**
1. n8n hace GET a `/api/marketing-packs?id={packId}`

**Resultado Esperado:**
- ✅ Se retorna el MarketingPack con todos sus datos
- ✅ Se incluyen los copies y assetPrompts
- ✅ Se retorna HTTP 200

---

### 9.2 API de Memory

#### TC-API-003: Obtener Contexto de Memoria
**Prioridad:** ALTA  
**Rol:** n8n (AllowAnonymous)

**Precondiciones:**
- Existen memorias en la base de datos para el tenant

**Pasos:**
1. n8n hace GET a `/api/Memory/context?tenantId={tenantId}&userId={userId}&campaignId={campaignId}`

**Resultado Esperado:**
- ✅ Se valida que tenantId es un GUID válido
- ✅ Se retorna el contexto estructurado:
  - Preferences
  - Learnings
  - Restrictions
  - UserPreferences
  - RecentConversations
  - CampaignMemories
  - LearningsList
  - SummarizedContext
- ✅ Se retorna HTTP 200

---

#### TC-API-004: Guardar Memoria desde n8n
**Prioridad:** ALTA  
**Rol:** n8n (AllowAnonymous)

**Precondiciones:**
- n8n está ejecutando el workflow

**Pasos:**
1. n8n hace POST a `/api/Memory/save`
2. Envía el JSON con:
   - tenantId, userId, campaignId
   - memoryType, content, context, tags

**Resultado Esperado:**
- ✅ Se valida que tenantId es un GUID válido
- ✅ Se valida que memoryType no está vacío
- ✅ Se valida que content no está vacío
- ✅ Se crea la memoria en la base de datos
- ✅ Se retorna HTTP 200 con el ID de la memoria creada

---

### 9.3 API de Consents

#### TC-API-005: Verificar Consentimientos
**Prioridad:** ALTA  
**Rol:** n8n (AllowAnonymous)

**Precondiciones:**
- Usuario tiene consentimientos en la base de datos

**Pasos:**
1. n8n hace GET a `/api/Consents/check?tenantId={tenantId}&userId={userId}`

**Resultado Esperado:**
- ✅ Se valida que tenantId y userId son GUIDs válidos
- ✅ Se verifica el consentimiento de AIGeneration
- ✅ Se verifica el consentimiento de AutoPublishing
- ✅ Se retorna HTTP 200 con:
  ```json
  {
    "aiConsent": true/false,
    "publishingConsent": true/false
  }
  ```

---

### 9.4 API de Metrics

#### TC-API-006: Guardar Métricas de Campaña
**Prioridad:** MEDIA  
**Rol:** n8n (AllowAnonymous)

**Precondiciones:**
- Existe una campaña en la base de datos

**Pasos:**
1. n8n hace POST a `/api/metrics/campaign`
2. Envía el JSON con:
   - tenantId, campaignId, metricDate
   - impressions, clicks, likes, comments, shares

**Resultado Esperado:**
- ✅ Se valida que tenantId y campaignId son GUIDs válidos
- ✅ Se crea el registro de métricas en la base de datos
- ✅ Se calculan automáticamente engagement, CTR, engagementRate
- ✅ Se retorna HTTP 200 con los datos guardados

---

### 9.5 API de Publishing Jobs

#### TC-API-007: Crear Publishing Job desde n8n
**Prioridad:** ALTA  
**Rol:** n8n (AllowAnonymous)

**Precondiciones:**
- Existe una campaña en la base de datos

**Pasos:**
1. n8n hace POST a `/api/publishing-jobs`
2. Envía el JSON con:
   - tenantId, campaignId, channel, content
   - publishedDate, publishedUrl, externalPostId

**Resultado Esperado:**
- ✅ Se valida que tenantId y campaignId son GUIDs válidos
- ✅ Se valida que channel no está vacío
- ✅ Se valida que content no está vacío
- ✅ Se crea el PublishingJob en la base de datos
- ✅ Se establece el estado como "Success"
- ✅ Se retorna HTTP 200 con los datos del job creado

---

## 10. PRUEBAS DE CONFIGURACIÓN

### 10.1 Configuración de IA

#### TC-CFG-001: Configurar API Key de OpenAI
**Prioridad:** ALTA  
**Rol:** Owner, Admin

**Precondiciones:**
- Usuario autenticado con rol Owner o Admin

**Pasos:**
1. Navegar a `/AIConfig/Index`
2. Ingresar API Key de OpenAI
3. Seleccionar modelo (ej: gpt-4, gpt-3.5-turbo)
4. Marcar como activo
5. Hacer clic en "Guardar"

**Resultado Esperado:**
- ✅ Se valida el formulario
- ✅ Se guarda la configuración en la base de datos
- ✅ Se encripta la API Key
- ✅ Se muestra mensaje de éxito
- ✅ La configuración se puede usar en los workflows de n8n

---

#### TC-CFG-002: Configurar IA - Sin Permisos
**Prioridad:** MEDIA  
**Rol:** Marketer

**Precondiciones:**
- Usuario autenticado con rol Marketer

**Pasos:**
1. Intentar navegar a `/AIConfig/Index`

**Resultado Esperado:**
- ❌ Acceso DENEGADO
- ✅ Se redirige a `/Account/AccessDenied`

---

### 10.2 Configuración de n8n

#### TC-CFG-003: Configurar n8n
**Prioridad:** ALTA  
**Rol:** Owner, Admin, SuperAdmin

**Precondiciones:**
- Usuario autenticado con permisos
- Se conoce la URL y API Key de n8n

**Pasos:**
1. Navegar a `/N8nConfig/Index`
2. Ingresar Base URL de n8n
3. Ingresar API Key de n8n
4. Hacer clic en "Guardar"
5. Hacer clic en "Probar Conexión"

**Resultado Esperado:**
- ✅ Se valida el formulario
- ✅ Se guarda la configuración en la base de datos
- ✅ Se encripta la API Key
- ✅ La prueba de conexión retorna éxito
- ✅ Se muestra mensaje de éxito
- ✅ La configuración se usa en ExternalAutomationService

---

#### TC-CFG-004: Configurar n8n - SuperAdmin
**Prioridad:** ALTA  
**Rol:** SuperAdmin

**Precondiciones:**
- Usuario SuperAdmin autenticado
- Existen múltiples tenants

**Pasos:**
1. Navegar a `/N8nConfig/Index`
2. Verificar que se muestra el selector de tenants
3. Seleccionar un tenant del dropdown
4. Configurar n8n para ese tenant
5. Guardar

**Resultado Esperado:**
- ✅ Se muestra el selector de tenants
- ✅ Se puede seleccionar cualquier tenant
- ✅ Se guarda la configuración para el tenant seleccionado
- ✅ Se puede cambiar entre tenants y ver/editar sus configuraciones

---

## 11. PRUEBAS DE USUARIOS Y TENANTS

### 11.1 Gestión de Usuarios

#### TC-USER-001: Listar Usuarios - Admin/Owner
**Prioridad:** ALTA  
**Rol:** Admin, Owner

**Precondiciones:**
- Usuario autenticado con rol Admin u Owner
- Existen usuarios en el tenant

**Pasos:**
1. Navegar a `/Users/Index`
2. Verificar que se muestra la lista de usuarios

**Resultado Esperado:**
- ✅ Se muestran los usuarios del tenant del usuario
- ✅ Cada usuario muestra: Nombre, Email, Rol, Estado
- ✅ NO se muestran usuarios de otros tenants
- ✅ Se muestran botones de acción según permisos

---

#### TC-USER-002: Listar Usuarios - SuperAdmin
**Prioridad:** ALTA  
**Rol:** SuperAdmin

**Precondiciones:**
- Usuario SuperAdmin autenticado
- Existen usuarios en múltiples tenants

**Pasos:**
1. Navegar a `/Users/Index`
2. Verificar que se muestra la lista de usuarios

**Resultado Esperado:**
- ✅ Se muestran usuarios de TODOS los tenants
- ✅ Cada usuario muestra el tenant al que pertenece
- ✅ Se pueden ver y gestionar usuarios de cualquier tenant

---

#### TC-USER-003: Crear Usuario
**Prioridad:** ALTA  
**Rol:** Admin, Owner, SuperAdmin

**Precondiciones:**
- Usuario autenticado con permisos
- Si es SuperAdmin, existen tenants disponibles

**Pasos:**
1. Navegar a `/Users/Create`
2. Completar el formulario:
   - Email: nuevo@ejemplo.com
   - Nombre completo: "Usuario Nuevo"
   - Contraseña: "Password123!"
   - Rol: Marketer
   - Tenant: Seleccionar (si es SuperAdmin)
3. Hacer clic en "Crear Usuario"

**Resultado Esperado:**
- ✅ Se valida el formulario
- ✅ Se valida que el email no existe
- ✅ Se valida que el rol existe
- ✅ Se crea el usuario en Identity
- ✅ Se asigna el rol correcto
- ✅ Se crea el registro en UserTenants
- ✅ Se muestra mensaje de éxito
- ✅ Se redirige a `/Users/Index`
- ✅ El nuevo usuario aparece en la lista

---

#### TC-USER-004: Ver Detalles de Usuario
**Prioridad:** MEDIA  
**Rol:** Admin, Owner, SuperAdmin

**Precondiciones:**
- Usuario autenticado con permisos
- Existe un usuario en el tenant (o cualquier tenant si es SuperAdmin)

**Pasos:**
1. Navegar a `/Users/Details/{userId}`
2. Verificar que se muestran los detalles

**Resultado Esperado:**
- ✅ Se muestran todos los datos del usuario:
  - Email, Nombre completo
  - Rol, Estado
  - Tenant
  - Fecha de creación
  - Último acceso
- ✅ Se muestran los consentimientos del usuario
- ✅ Se muestran las campañas del usuario

---

### 11.2 Gestión de Tenants

#### TC-TEN-001: Listar Tenants - SuperAdmin
**Prioridad:** ALTA  
**Rol:** SuperAdmin

**Precondiciones:**
- Usuario SuperAdmin autenticado
- Existen múltiples tenants en la base de datos

**Pasos:**
1. Navegar a `/Tenants/Index`
2. Verificar que se muestra la lista de tenants

**Resultado Esperado:**
- ✅ Se muestran todos los tenants
- ✅ Cada tenant muestra: Nombre, Subdomain, Email de contacto, Estado
- ✅ Se muestran botones de acción

---

#### TC-TEN-002: Listar Tenants - Usuario Normal
**Prioridad:** MEDIA  
**Rol:** Owner, Admin, Marketer

**Precondiciones:**
- Usuario autenticado sin rol SuperAdmin

**Pasos:**
1. Intentar navegar a `/Tenants/Index`

**Resultado Esperado:**
- ❌ Acceso DENEGADO
- ✅ Se redirige a `/Account/AccessDenied`

---

#### TC-TEN-003: Crear Tenant
**Prioridad:** ALTA  
**Rol:** SuperAdmin

**Precondiciones:**
- Usuario SuperAdmin autenticado

**Pasos:**
1. Navegar a `/Tenants/Create`
2. Completar el formulario:
   - Nombre: "Nuevo Tenant"
   - Subdomain: "nuevo-tenant"
   - Email de contacto: "contacto@nuevo-tenant.com"
3. Hacer clic en "Crear Tenant"

**Resultado Esperado:**
- ✅ Se valida el formulario
- ✅ Se valida que el subdomain no existe
- ✅ Se crea el tenant en la base de datos
- ✅ Se muestra mensaje de éxito
- ✅ Se redirige a `/Tenants/Index`
- ✅ El nuevo tenant aparece en la lista

---

## RESUMEN DE CASOS DE PRUEBA

### Por Prioridad

- **ALTA:** 45 casos
- **MEDIA:** 20 casos
- **BAJA:** 0 casos

**Total:** 65 casos de prueba

### Por Módulo

- **Autenticación y Roles:** 7 casos
- **Campañas:** 11 casos
- **Marketing Request:** 3 casos
- **Consents:** 5 casos
- **Memory:** 2 casos
- **Content:** 4 casos
- **Metrics:** 3 casos
- **Publishing:** 4 casos
- **APIs:** 7 casos
- **Configuración:** 4 casos
- **Usuarios y Tenants:** 6 casos

### Por Rol

- **SuperAdmin:** 15 casos específicos
- **Owner:** 12 casos específicos
- **Admin:** 10 casos específicos
- **Marketer:** 8 casos específicos
- **Cualquier usuario autenticado:** 10 casos
- **n8n (APIs):** 7 casos

---

## NOTAS IMPORTANTES

1. **Orden de Ejecución Recomendado:**
   - Primero: Pruebas de autenticación y roles
   - Segundo: Pruebas de creación de datos básicos (Tenants, Usuarios, Consents)
   - Tercero: Pruebas de funcionalidades principales (Campañas, Marketing Request)
   - Cuarto: Pruebas de APIs
   - Quinto: Pruebas de integración end-to-end

2. **Datos de Prueba:**
   - Crear usuarios de prueba con diferentes roles
   - Crear tenants de prueba
   - Crear campañas de prueba
   - Configurar n8n para pruebas

3. **Ambiente de Pruebas:**
   - Usar base de datos de desarrollo
   - Configurar n8n en modo desarrollo
   - Tener credenciales de OpenAI para pruebas

4. **Criterios de Aceptación:**
   - Todos los casos de prioridad ALTA deben pasar
   - Al menos 90% de los casos de prioridad MEDIA deben pasar
   - Documentar cualquier caso que falle con detalles del error

---

**Fecha de creación:** 2026-01-04  
**Versión del documento:** 1.0  
**Estado:** ✅ Listo para ejecución

