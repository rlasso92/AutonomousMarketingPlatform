# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first (for better layer caching)
COPY src/AutonomousMarketingPlatform.Domain/AutonomousMarketingPlatform.Domain.csproj src/AutonomousMarketingPlatform.Domain/
COPY src/AutonomousMarketingPlatform.Application/AutonomousMarketingPlatform.Application.csproj src/AutonomousMarketingPlatform.Application/
COPY src/AutonomousMarketingPlatform.Infrastructure/AutonomousMarketingPlatform.Infrastructure.csproj src/AutonomousMarketingPlatform.Infrastructure/
COPY src/AutonomousMarketingPlatform.Web/AutonomousMarketingPlatform.Web.csproj src/AutonomousMarketingPlatform.Web/

# Restore dependencies for the Web project (which will restore all dependencies)
WORKDIR /src/src/AutonomousMarketingPlatform.Web
RUN dotnet restore

# Copy everything else and build
COPY . .
WORKDIR /src/src/AutonomousMarketingPlatform.Web

# Asegurar que las vistas Razor se incluyan explícitamente en el publish
# El SDK incluye automáticamente Views y wwwroot, pero aseguramos que se copien
RUN dotnet publish AutonomousMarketingPlatform.Web.csproj -c Release -o /app/publish --no-restore \
    /p:CopyRazorGenerateFilesToPublishDirectory=true \
    /p:CopyRefAssembliesToPublishDirectory=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published app (esto incluye DLLs, configs, etc.)
COPY --from=build /app/publish .

# Copiar explícitamente Views y wwwroot desde el build stage
# Esto asegura que estén disponibles incluso si no se copiaron en el publish
COPY --from=build /src/src/AutonomousMarketingPlatform.Web/Views ./Views
COPY --from=build /src/src/AutonomousMarketingPlatform.Web/wwwroot ./wwwroot

# Expose port (Render will set PORT env var dynamically)
EXPOSE 8080

# Set environment variables
# Note: ASPNETCORE_URLS will be overridden by Render's $PORT env var
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the app
ENTRYPOINT ["dotnet", "AutonomousMarketingPlatform.Web.dll"]

