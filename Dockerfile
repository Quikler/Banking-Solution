# 1. Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

# 2. Build image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Development
WORKDIR /src

COPY ["Banking-Solution.sln", "."]
COPY ["src/WebApi/WebApi.csproj", "./WebApi/"]
COPY ["src/DAL/DAL.csproj", "./DAL/"]
COPY ["src/BAL/BAL.csproj", "./BAL/"]
COPY ["src/Contracts/Contracts.csproj", "./Contracts/"]
COPY ["src/Common/Common.csproj", "./Common/"]

COPY . .

# Restore sln
RUN dotnet restore "Banking-Solution.sln"

# Build + publish
RUN dotnet publish "src/WebApi/WebApi.csproj" -c Release -o /web-api/publish/web-api

# 3. Runtime images
FROM base AS web-api
WORKDIR /web-api
COPY --from=build /web-api/publish/web-api .
ENTRYPOINT ["dotnet", "WebApi.dll"]
