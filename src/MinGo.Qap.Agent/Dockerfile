FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["MinGo.Qap.Agent.csproj", "."]
COPY ["..\MinGo.Qap.Shared\MinGo.Qap.Shared.csproj", "..\MinGo.Qap.Shared\"]
RUN dotnet restore "./MinGo.Qap.Agent.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "MinGo.Qap.Agent.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MinGo.Qap.Agent.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MinGo.Qap.Agent.dll"]
