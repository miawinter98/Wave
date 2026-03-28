

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN mkdir -p /app/configuration
VOLUME /app/configuration
EXPOSE 8080

WORKDIR /app
USER app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG VERSION=0.0.1
WORKDIR /src
COPY wave.server/wave.server.csproj ./wave.server/wave.server.csproj
RUN dotnet restore ./wave.server/wave.server.csproj

COPY wave.server/ wave.server/
WORKDIR /src/wave.server
RUN dotnet build "./wave.server.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build \
    -p:VersionPrefix="${VERSION}"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG VERSION=0.0.1
RUN dotnet publish "./wave.server.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    -p:UseAppHost=false \
    -p:VersionPrefix="${VERSION}"

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY LICENSE .
ENTRYPOINT ["dotnet", "wave.server.dll"]