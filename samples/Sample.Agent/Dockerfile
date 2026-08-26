# =============================================================================
# Sample.Agent Dockerfile
# 用于部署 Qap Agent 实例
# =============================================================================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore (cached layer)
COPY samples/Sample.Agent/Sample.Agent.csproj samples/Sample.Agent/
COPY src/MinGo.Qap.Agent/MinGo.Qap.Agent.csproj src/MinGo.Qap.Agent/
COPY src/MinGo.Qap.Shared/MinGo.Qap.Shared.csproj src/MinGo.Qap.Shared/
RUN dotnet restore samples/Sample.Agent/Sample.Agent.csproj

# Copy source and publish
COPY . .
WORKDIR /src/samples/Sample.Agent
RUN dotnet publish Sample.Agent.csproj -c Release -o /app/publish /p:UseAppHost=false

# =============================================================================
# Runtime
# =============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80

ENTRYPOINT ["dotnet", "Sample.Agent.dll"]
