FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/CardActions.Api/CardActions.Api.csproj", "src/CardActions.Api/"]
COPY ["src/CardActions.Domain/CardActions.Domain.csproj", "src/CardActions.Domain/"]
COPY ["src/CardActions.Infrastructure/CardActions.Infrastructure.csproj", "src/CardActions.Infrastructure/"]
RUN dotnet restore "src/CardActions.Api/CardActions.Api.csproj"
COPY . .
WORKDIR "/src/src/CardActions.Api"
RUN dotnet build "CardActions.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CardActions.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CardActions.Api.dll"] 