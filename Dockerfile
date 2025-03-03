FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app
COPY . .

# Create non-root user and set permissions
RUN adduser --disabled-password \
    --home /app \
    --gecos "" dotnetuser && \
    chown -R dotnetuser:dotnetuser /app

# Switch to non-root user
USER dotnetuser

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CardActions.Api.dll"]

LABEL org.opencontainers.image.version=${VERSION}
LABEL org.opencontainers.image.title="Card Actions API"
LABEL org.opencontainers.image.description="API do zarządzania akcjami dla kart płatniczych"
LABEL org.opencontainers.image.source="https://github.com/leszekszpunar/CardActions.Api" 