# Etapa de construcción
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar la solución y restaurar las dependencias
COPY sga_back.sln .
COPY sga_back/sga_back.csproj ./sga_back/
RUN dotnet restore sga_back.sln

# Copiar el resto de los archivos y compilar
COPY . .
RUN dotnet publish -c Release -o out

# Etapa de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copiar la aplicación compilada
COPY --from=build /app/out .

# Configurar ASP.NET Core para escuchar en el puerto 80
ENV ASPNETCORE_URLS=http://+:8081

# Exponer el puerto 80
EXPOSE 8081

# Ejecutar la aplicación
ENTRYPOINT ["dotnet", "sga_back.dll"]
