FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AutoProxy.Server/AutoProxy.Server.csproj", "AutoProxy.Server/"]
RUN dotnet restore "AutoProxy.Server/AutoProxy.Server.csproj"
COPY . .
WORKDIR "/src/AutoProxy.Server"
RUN dotnet build "AutoProxy.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AutoProxy.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AutoProxy.Server.dll"]
