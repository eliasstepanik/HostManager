FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AutoProxy.PortManager/AutoProxy.PortManager.csproj", "AutoProxy.PortManager/"]
RUN dotnet restore "AutoProxy.PortManager/AutoProxy.PortManager.csproj"
COPY . .
WORKDIR "/src/AutoProxy.PortManager"
RUN dotnet build "AutoProxy.PortManager.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AutoProxy.PortManager.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AutoProxy.PortManager.dll"]
