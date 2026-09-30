# syntax=docker/dockerfile:1

# ---- Build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restauration en cache séparé des sources
COPY Directory.Build.props Directory.Packages.props ./
COPY src/UsMecanic.Domain/UsMecanic.Domain.csproj src/UsMecanic.Domain/
COPY src/UsMecanic.Application/UsMecanic.Application.csproj src/UsMecanic.Application/
COPY src/UsMecanic.Infrastructure/UsMecanic.Infrastructure.csproj src/UsMecanic.Infrastructure/
COPY src/UsMecanic.Api/UsMecanic.Api.csproj src/UsMecanic.Api/
RUN dotnet restore src/UsMecanic.Api/UsMecanic.Api.csproj

COPY .editorconfig ./
COPY src/ src/
RUN dotnet publish src/UsMecanic.Api/UsMecanic.Api.csproj -c Release -o /app/publish --no-restore

# ---- Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
  CMD wget -qO- http://127.0.0.1:8080/health || exit 1

ENTRYPOINT ["dotnet", "UsMecanic.Api.dll"]
