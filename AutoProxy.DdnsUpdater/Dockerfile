FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AutoProxy.BaseClient/AutoProxy.DdnsUpdater.csproj", "AutoProxy.DdnsUpdater/"]
RUN dotnet restore "AutoProxy.BaseClient/AutoProxy.DdnsUpdater.csproj"
COPY . .
WORKDIR "/src/AutoProxy.DdnsUpdater"
RUN dotnet build "AutoProxy.DdnsUpdater.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AutoProxy.DdnsUpdater.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AutoProxy.DdnsUpdater.dll"]
