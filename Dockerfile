# =============================================================================
# Stage 1: UI Build (Node.js)
# =============================================================================
FROM swr.cn-north-4.myhuaweicloud.com/ddn-k8s/docker.io/library/node:20-alpine AS ui-build
WORKDIR /ui

# Install pnpm (bypass corepack for Node 20 compatibility)
RUN npm install -g pnpm

# Install dependencies (cached layer)
COPY src/MinGo.Qap.UI/package.json src/MinGo.Qap.UI/pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

# Build UI
COPY src/MinGo.Qap.UI/ .
RUN pnpm build

# =============================================================================
# Stage 2: .NET Build
# =============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS dotnet-build
WORKDIR /src

# Restore NuGet packages (cached layer)
COPY src/MinGo.Qap.Platform/MinGo.Qap.Platform.csproj src/MinGo.Qap.Platform/
COPY src/MinGo.Qap.Shared/MinGo.Qap.Shared.csproj src/MinGo.Qap.Shared/
RUN dotnet restore src/MinGo.Qap.Platform/MinGo.Qap.Platform.csproj

# Copy full source
COPY . .

# Copy UI build output into wwwroot
COPY --from=ui-build /ui/dist src/MinGo.Qap.Platform/wwwroot/

# Publish
WORKDIR /src/src/MinGo.Qap.Platform
RUN dotnet publish MinGo.Qap.Platform.csproj -c Release -o /app/publish /p:UseAppHost=false

# =============================================================================
# Stage 3: Runtime
# =============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=dotnet-build /app/publish .
EXPOSE 80

ENTRYPOINT ["dotnet", "MinGo.Qap.Platform.dll"]
