#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY Helicopters_Russia.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o Helicopters_Russia
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/Helicopters_Russia ./
ENTRYPOINT ["dotnet", "Helicopters_Russia.dll"]

