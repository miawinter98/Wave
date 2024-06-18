#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

ARG BASE=
FROM mcr.microsoft.com/dotnet/aspnet:8.0$BASE AS base
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN mkdir -p /app/files && chown app /app/files
RUN if command -v apt-get; then \
	apt-get update && apt-get install -y curl; \
	else \
	apk add --update curl icu-libs icu-data-full tzdata; \
	fi
USER app
VOLUME /app/files
VOLUME /configuration
WORKDIR /app
EXPOSE 8080
HEALTHCHECK --start-period=5s --start-interval=15s --interval=30s --timeout=30s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

FROM node:20-alpine AS vite-build
WORKDIR /src
RUN mkdir -p "wwwroot"
COPY ["Wave/package.json", "Wave/package-lock.json", "./"]
RUN npm install
COPY [ \
    "Wave/tsconfig.json", \
    "Wave/tsconfig.node.json", \
    "Wave/tailwind.config.ts", \
    "Wave/vite.config.ts", \
    "./"]
COPY ["Wave/Assets/", "./Assets/"]
# need to copy website files, otherwise tailwind doesn't compile 
# the required classes
COPY ["Wave/Components/", "./Components/"]
RUN npx vite build

FROM mcr.microsoft.com/dotnet/sdk:8.0$BASE AS build
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
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
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
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
COPY --from=vite-build /src/wwwroot ./wwwroot
COPY --from=publish /app/publish .
COPY LICENSE .
ENTRYPOINT ["dotnet", "Wave.dll"]
