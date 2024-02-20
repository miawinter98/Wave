#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN mkdir -p /app/files && chown app /app/files
RUN mkdir -p /configuration && chown app /configuration
RUN apt-get update && apt-get install -y curl
USER app
VOLUME /app/files
VOLUME /configuration
WORKDIR /app
EXPOSE 8080
HEALTHCHECK --start-period=5s --start-interval=15s --interval=30s --timeout=30s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG VERSION=0.0.1
WORKDIR /src
COPY ["Wave/Wave.csproj", "Wave/"]
RUN dotnet restore "./Wave/Wave.csproj"
COPY . .
WORKDIR "/src/Wave"
RUN dotnet build "./Wave.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build \
    -p:Version="${VERSION}"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG VERSION=0.0.1
ARG VERSION_SUFFIX=
RUN dotnet publish "./Wave.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    -p:UseAppHost=false \
    -p:Version="${VERSION}"

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Wave.dll"]
