FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/MinGo.Qap.Platform/MinGo.Qap.Platform.csproj", "src/MinGo.Qap.Platform/"]
COPY ["src/MinGo.Qap.Shared/MinGo.Qap.Shared.csproj", "src/MinGo.Qap.Shared/"]
RUN dotnet restore "src/MinGo.Qap.Platform/MinGo.Qap.Platform.csproj"
COPY . .
WORKDIR "/src/src/MinGo.Qap.Platform/"
RUN dotnet build "MinGo.Qap.Platform.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MinGo.Qap.Platform.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MinGo.Qap.Platform.dll"]
