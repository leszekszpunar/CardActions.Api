FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG VERSION=0.0.0
WORKDIR /src
COPY ["src/CardActions.Api/CardActions.Api.csproj", "src/CardActions.Api/"]
COPY ["src/CardActions.Application/CardActions.Application.csproj", "src/CardActions.Application/"]
COPY ["src/CardActions.Domain/CardActions.Domain.csproj", "src/CardActions.Domain/"]
COPY ["src/CardActions.Infrastructure/CardActions.Infrastructure.csproj", "src/CardActions.Infrastructure/"]
COPY ["src/CardActions.Infrastructure.Data/CardActions.Infrastructure.Data.csproj", "src/CardActions.Infrastructure.Data/"]
RUN dotnet restore "src/CardActions.Api/CardActions.Api.csproj"
COPY . .
WORKDIR "/src/src/CardActions.Api"
RUN dotnet build "CardActions.Api.csproj" -c Release -o /app/build /p:Version=$VERSION

FROM build AS publish
ARG VERSION
RUN dotnet publish "CardActions.Api.csproj" -c Release -o /app/publish /p:Version=$VERSION

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CardActions.Api.dll"]

LABEL org.opencontainers.image.version=${VERSION}
LABEL org.opencontainers.image.title="Card Actions API"
LABEL org.opencontainers.image.description="API do zarządzania akcjami dla kart płatniczych"
LABEL org.opencontainers.image.source="https://github.com/leszekszpunar/CardActions.Api" 