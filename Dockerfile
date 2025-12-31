# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln .
COPY src/AutonomousMarketingPlatform.Domain/AutonomousMarketingPlatform.Domain.csproj src/AutonomousMarketingPlatform.Domain/
COPY src/AutonomousMarketingPlatform.Application/AutonomousMarketingPlatform.Application.csproj src/AutonomousMarketingPlatform.Application/
COPY src/AutonomousMarketingPlatform.Infrastructure/AutonomousMarketingPlatform.Infrastructure.csproj src/AutonomousMarketingPlatform.Infrastructure/
COPY src/AutonomousMarketingPlatform.Web/AutonomousMarketingPlatform.Web.csproj src/AutonomousMarketingPlatform.Web/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . .

# Build and publish
WORKDIR /src/src/AutonomousMarketingPlatform.Web
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Expose port (Render will set PORT env var dynamically)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# Run the app
ENTRYPOINT ["dotnet", "AutonomousMarketingPlatform.Web.dll"]
