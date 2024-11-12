#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
#WORKDIR /app
#USER app
#
#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#COPY ["Helicopters_Russia.csproj", "."]
#RUN dotnet restore "./././Helicopters_Russia.csproj"
#COPY . .
#WORKDIR "/src/."
#RUN dotnet build "./Helicopters_Russia.csproj" -c $BUILD_CONFIGURATION -o /app/build
#
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "./Helicopters_Russia.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "Helicopters_Russia.dll"]

# Стадия сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файл проекта и восстанавливаем зависимости
COPY ["Helicopters_Russia.csproj", "./"]
RUN dotnet restore

# Копируем остальные файлы и выполняем публикацию
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-warn


# Стадия исполнения
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Задаем точку входа
ENTRYPOINT ["dotnet", "Helicopters_Russia.dll"]
