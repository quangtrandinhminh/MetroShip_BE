FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /MetroShip/

COPY ["src/WebAPI/MetroShip.WebAPI.csproj", "WebAPI/"]
COPY ["src/Service/MetroShip.Service.csproj", "Service/"]
COPY ["src/Repository/MetroShip.Repository.csproj", "Repository/"]
COPY ["src/Utility/MetroShip.Utility.csproj", "Utility/"]
RUN dotnet restore "./WebAPI/./MetroShip.WebAPI.csproj"

COPY . .
WORKDIR "/MetroShip/src/WebAPI"
RUN dotnet build "./MetroShip.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MetroShip.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MetroShip.WebAPI.dll"]
