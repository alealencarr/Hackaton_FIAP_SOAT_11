FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Instalar FFmpeg
RUN apt-get update && apt-get install -y ffmpeg && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar arquivos de projeto
COPY ["src/FiapX.API/FiapX.API.csproj", "src/FiapX.API/"]
COPY ["src/FiapX.Application/FiapX.Application.csproj", "src/FiapX.Application/"]
COPY ["src/FiapX.Domain/FiapX.Domain.csproj", "src/FiapX.Domain/"]
COPY ["src/FiapX.Infrastructure/FiapX.Infrastructure.csproj", "src/FiapX.Infrastructure/"]
COPY ["src/FiapX.Shared/FiapX.Shared.csproj", "src/FiapX.Shared/"]

# Restaurar dependências
RUN dotnet restore "src/FiapX.API/FiapX.API.csproj"

# Copiar código fonte
COPY . .

# Build
WORKDIR "/src/src/FiapX.API"
RUN dotnet build "FiapX.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FiapX.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Criar diretórios de upload e output
RUN mkdir -p /app/uploads /app/outputs /app/logs

ENTRYPOINT ["dotnet", "FiapX.API.dll"]
