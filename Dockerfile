# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/PeaceNest.Api/PeaceNest.Api.csproj src/PeaceNest.Api/
RUN dotnet restore src/PeaceNest.Api/PeaceNest.Api.csproj

COPY src/PeaceNest.Api/ src/PeaceNest.Api/
WORKDIR /src/src/PeaceNest.Api
RUN dotnet publish PeaceNest.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0

COPY --from=build /app/publish ./

EXPOSE 8080

ENTRYPOINT ["sh", "-c", "dotnet PeaceNest.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
