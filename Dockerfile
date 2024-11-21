FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY Helicopters_Russia.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish Helicopters_Russia.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
RUN mkdir -p /initialxlsx

ENTRYPOINT ["dotnet", "Helicopters_Russia.dll"]
