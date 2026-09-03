# File path in project: RideHailing.API/Dockerfile
# Build context is this folder (see docker-compose.yml: build.context: ./RideHailing.API)

# ── Build stage ────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first, on just the csproj, so `docker compose build` only
# re-runs restore when dependencies actually change, not on every code edit.
COPY RideHailing.API.csproj ./
RUN dotnet restore RideHailing.API.csproj

COPY . .
RUN dotnet publish RideHailing.API.csproj -c Release -o /app/publish --no-restore

# ── Runtime stage ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# .NET's official images listen on 8080 by default for non-root containers;
# docker-compose.yml maps host 5000 -> 8080 so the mobile app's existing
# http://localhost:5000 base URL keeps working unchanged.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "RideHailing.API.dll"]