#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Oqtane.Server/Oqtane.Server.csproj", "Oqtane.Server/"]
COPY ["Oqtane.Client/Oqtane.Client.csproj", "Oqtane.Client/"]
COPY ["Oqtane.Shared/Oqtane.Shared.csproj", "Oqtane.Shared/"]
COPY ["Oqtane.Database.MySQL/Oqtane.Database.MySQL.csproj", "Oqtane.Database.MySQL/"]
COPY ["Oqtane.Database.PostgreSQL/Oqtane.Database.PostgreSQL.csproj", "Oqtane.Database.PostgreSQL/"]
COPY ["Oqtane.Database.Sqlite/Oqtane.Database.Sqlite.csproj", "Oqtane.Database.Sqlite/"]
COPY ["Oqtane.Database.SqlServer/Oqtane.Database.SqlServer.csproj", "Oqtane.Database.SqlServer/"]
RUN dotnet restore "./Oqtane.Database.MySQL/Oqtane.Database.MySQL.csproj"
RUN dotnet restore "./Oqtane.Database.PostgreSQL/Oqtane.Database.PostgreSQL.csproj"
RUN dotnet restore "./Oqtane.Database.Sqlite/Oqtane.Database.Sqlite.csproj"
RUN dotnet restore "./Oqtane.Database.SqlServer/Oqtane.Database.SqlServer.csproj"
RUN dotnet restore "./Oqtane.Server/Oqtane.Server.csproj"
COPY . .
WORKDIR "/src/Oqtane.Database.MySQL"
RUN dotnet build "./Oqtane.Database.MySQL.csproj" -c $BUILD_CONFIGURATION
WORKDIR "/src/Oqtane.Database.PostgreSQL"
RUN dotnet build "./Oqtane.Database.PostgreSQL.csproj" -c $BUILD_CONFIGURATION
WORKDIR "/src/Oqtane.Database.Sqlite"
RUN dotnet build "./Oqtane.Database.Sqlite.csproj" -c $BUILD_CONFIGURATION
WORKDIR "/src/Oqtane.Database.SqlServer"
RUN dotnet build "./Oqtane.Database.SqlServer.csproj" -c $BUILD_CONFIGURATION
WORKDIR "/src/Oqtane.Server"
RUN dotnet build "./Oqtane.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Oqtane.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Oqtane.Server.dll"]