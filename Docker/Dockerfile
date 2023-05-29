FROM mcr.microsoft.com/dotnet/aspnet:7.0.5-bullseye-slim-amd64 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR .
COPY ["Endpoint/Endpoint.csproj", "Endpoint/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Consumers/Consumers.csproj", "Consumers/"]
COPY ["CronJob/CronJob.csproj", "CronJob/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["GrpcServices/GrpcServices.csproj", "GrpcServices/"]
COPY ["Migration/Migration.csproj", "Migration/"]
COPY ["Options/Options.csproj", "Options/"]
COPY ["Postgres/Postgres.csproj", "Postgres/"]
COPY ["Redis/Redis.csproj", "Redis/"]
COPY ["Serializator/Serializator.csproj", "Serializator/"]
RUN dotnet restore "Endpoint/Endpoint.csproj"
COPY . .
RUN dotnet build "Endpoint/Endpoint.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Endpoint/Endpoint.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Endpoint.dll"]
