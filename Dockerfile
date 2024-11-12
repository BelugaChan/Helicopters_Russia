#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Helicopters_Russia.csproj", "."]
RUN dotnet restore "./././Helicopters_Russia.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./Helicopters_Russia.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Helicopters_Russia.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Helicopters_Russia.dll"]
#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#WORKDIR /app
#
#COPY *.csproj ./
#RUN dotnet restore
#
#COPY . ./
#RUN dotnet publish -c Release -o out
#
#FROM mcr.microsoft.com/dotnet/runtime:8.0
#WORKDIR /app
#COPY --from=build /app/out .
#
#ENTRYPOINT ["dotnet", "Helicopters_Russia.dll"]