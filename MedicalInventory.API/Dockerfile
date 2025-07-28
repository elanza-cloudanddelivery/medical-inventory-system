# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar el csproj y restaurar dependencias
COPY *.csproj ./
RUN dotnet restore

# Copiar todo el código fuente
COPY . ./

# Publicar la app en modo Release
RUN dotnet publish -c Release -o /app/publish

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copiar la app publicada desde la etapa anterior
COPY --from=build /app/publish .

# Copiar la base de datos SQLite a la raíz de la app en el contenedor
COPY medical_inventory.db .

# Configurar la app para que escuche en el puerto 80
ENV ASPNETCORE_URLS=http://+:80

# Exponer puerto 80
EXPOSE 80

# Comando para ejecutar la app
ENTRYPOINT ["dotnet", "MedicalInventory.API.dll"]
