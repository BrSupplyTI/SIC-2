# Etapa 1: build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["SIC.Web/SIC.Web.csproj", "SIC.Web/"]
COPY ["SIC.Domain/SIC.Domain.csproj", "SIC.Domain/"]
COPY ["SIC.Infrastructure/SIC.Infrastructure.csproj", "SIC.Infrastructure/"]

RUN dotnet restore "SIC.Web/SIC.Web.csproj"

COPY . .
RUN dotnet publish "SIC.Web/SIC.Web.csproj" -c Release -o /app/publish

# Etapa 2: imagem final (menor, sem SDK)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SIC.Web.dll"]