# Script para hacer backup de la base de datos PostgreSQL local
# Autor: Auto-generated
# Fecha: $(Get-Date -Format "yyyy-MM-dd")

param(
    [string]$OutputPath = ".\backups",
    [string]$ConfigPath = ".\src\AutonomousMarketingPlatform.Web\appsettings.json"
)

# Función para parsear la connection string
function Parse-ConnectionString {
    param([string]$ConnectionString)
    
    $result = @{}
    $parts = $ConnectionString -split ';'
    
    foreach ($part in $parts) {
        if ($part -match '^(\w+)=(.*)$') {
            $key = $matches[1]
            $value = $matches[2]
            $result[$key] = $value
        }
    }
    
    return $result
}

# Verificar que existe el archivo de configuración
if (-not (Test-Path $ConfigPath)) {
    Write-Error "No se encontró el archivo de configuración en: $ConfigPath"
    exit 1
}

# Leer la configuración
try {
    $config = Get-Content $ConfigPath | ConvertFrom-Json
    $connectionString = $config.ConnectionStrings.DefaultConnection
    
    if (-not $connectionString) {
        Write-Error "No se encontró la cadena de conexión 'DefaultConnection' en el archivo de configuración"
        exit 1
    }
    
    Write-Host "Cadena de conexión encontrada" -ForegroundColor Green
} catch {
    Write-Error "Error al leer el archivo de configuración: $_"
    exit 1
}

# Parsear la connection string
$connParams = Parse-ConnectionString -ConnectionString $connectionString

$dbHost = $connParams['Host']
$port = if ($connParams['Port']) { $connParams['Port'] } else { '5432' }
$database = $connParams['Database']
$username = $connParams['Username']
$password = $connParams['Password']

Write-Host "Parámetros de conexión:" -ForegroundColor Cyan
Write-Host "  Host: $dbHost"
Write-Host "  Port: $port"
Write-Host "  Database: $database"
Write-Host "  Username: $username"

# Buscar pg_dump en ubicaciones comunes
$pgDumpPath = $null
$possiblePaths = @(
    "pg_dump",
    "C:\Program Files\PostgreSQL\*\bin\pg_dump.exe",
    "C:\Program Files (x86)\PostgreSQL\*\bin\pg_dump.exe"
)

foreach ($path in $possiblePaths) {
    if ($path -eq "pg_dump") {
        $pgDumpCmd = Get-Command pg_dump -ErrorAction SilentlyContinue
        if ($pgDumpCmd) {
            $pgDumpPath = $pgDumpCmd.Source
            break
        }
    } else {
        $found = Get-ChildItem -Path $path -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($found) {
            $pgDumpPath = $found.FullName
            break
        }
    }
}

if (-not $pgDumpPath) {
    Write-Error "pg_dump no está disponible. Asegúrate de que PostgreSQL esté instalado y en el PATH."
    Write-Host "Intenta agregar PostgreSQL al PATH o especifica la ruta completa a pg_dump.exe" -ForegroundColor Yellow
    exit 1
}

Write-Host "`npg_dump encontrado en: $pgDumpPath" -ForegroundColor Green

# Crear directorio de backups si no existe
if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
    Write-Host "Directorio de backups creado: $OutputPath" -ForegroundColor Green
}

# Generar nombre del archivo de backup con timestamp
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupFileName = "AutonomousMarketingPlatform_backup_$timestamp.sql"
$backupPath = Join-Path $OutputPath $backupFileName

Write-Host "`nIniciando backup..." -ForegroundColor Cyan
Write-Host "Archivo de destino: $backupPath" -ForegroundColor Yellow

# Configurar variable de entorno para la contraseña
$env:PGPASSWORD = $password

try {
    # Ejecutar pg_dump
    $pgDumpArgs = @(
        "-h", $dbHost,
        "-p", $port,
        "-U", $username,
        "-d", $database,
        "-F", "p",  # Formato plain text (SQL)
        "-f", $backupPath,
        "--verbose",
        "--no-owner",
        "--no-privileges"
    )
    
    & $pgDumpPath $pgDumpArgs
    
    if ($LASTEXITCODE -eq 0) {
        $fileSize = (Get-Item $backupPath).Length / 1MB
        Write-Host "`n[OK] Backup completado exitosamente!" -ForegroundColor Green
        Write-Host "  Archivo: $backupPath" -ForegroundColor White
        Write-Host "  Tamaño: $([math]::Round($fileSize, 2)) MB" -ForegroundColor White
        
        # También crear un backup comprimido
        $compressedBackupPath = $backupPath -replace '\.sql$', '.zip'
        Write-Host "`nComprimiendo backup..." -ForegroundColor Cyan
        Compress-Archive -Path $backupPath -DestinationPath $compressedBackupPath -Force
        $compressedSize = (Get-Item $compressedBackupPath).Length / 1MB
        Write-Host "[OK] Backup comprimido creado: $compressedBackupPath" -ForegroundColor Green
        Write-Host "  Tamaño comprimido: $([math]::Round($compressedSize, 2)) MB" -ForegroundColor White
    } else {
        Write-Error "Error al ejecutar pg_dump. Código de salida: $LASTEXITCODE"
        exit 1
    }
} catch {
    Write-Error "Error durante el backup: $_"
    exit 1
} finally {
    # Limpiar la variable de entorno
    Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
}

Write-Host "`n[OK] Proceso completado!" -ForegroundColor Green

