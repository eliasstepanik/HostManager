FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["PortUpdate/PortUpdate.csproj", "PortUpdate/"]
RUN dotnet restore "PortUpdate/PortUpdate.csproj"
COPY . .
WORKDIR "/src/PortUpdate"
RUN dotnet build "PortUpdate.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PortUpdate.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PortUpdate.dll"]
