FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src

COPY ["HartaRPA.sln", ""]
COPY ["BuildingBlocks/EventBus/EventBus/EventBus.csproj", "BuildingBlocks/EventBus/EventBus/"]
COPY ["BuildingBlocks/EventBus/EventBusRabbitMQ/EventBusRabbitMQ.csproj", "BuildingBlocks/EventBus/EventBusRabbitMQ/"]
COPY ["BuildingBlocks/EventBus/EFIntegrationEventLog/EFIntegrationEventLog.csproj", "BuildingBlocks/EventBus/EFIntegrationEventLog/"]
COPY ["BuildingBlocks/WebHostCustomization/WebHost.Customization/WebHost.Customization.csproj", "BuildingBlocks/WebHostCustomization/WebHost.Customization/"]
COPY ["Services/File/File.API/File.API.csproj", "Services/File/File.API/"]
COPY ["Services/File/File.FunctionalTests/File.FunctionalTests.csproj", "Services/File/File.FunctionalTests/"]
COPY ["Services/File/File.UnitTests/File.UnitTests.csproj", "Services/File/File.UnitTests/"]
COPY ["Services/Ordering/Ordering.API/Ordering.API.csproj", "Services/Ordering/Ordering.API/"]
COPY ["Services/Ordering/Ordering.Domain/Ordering.Domain.csproj", "Services/Ordering/Ordering.Domain/"]
COPY ["Services/Ordering/Ordering.Infrastructure/Ordering.Infrastructure.csproj", "Services/Ordering/Ordering.Infrastructure/"]
COPY ["docker-compose.dcproj", ""]
COPY ["NuGet.config", ""]

RUN dotnet restore "HartaRPA.sln"

COPY . .
WORKDIR "/src/Services/Ordering/Ordering.API"
RUN dotnet publish --no-restore -c Release -o /app

FROM build AS publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Ordering.API.dll"]