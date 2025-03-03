# Card Actions API

API do zarządzania akcjami dla kart płatniczych, zbudowane z wykorzystaniem .NET 8 i Clean Architecture.

## 🏗️ Architektura

Projekt wykorzystuje Clean Architecture i wzorzec CQRS, podzielony na warstwy:

```
src/
├── CardActions.Api           # Warstwa prezentacji (REST API)
├── CardActions.Application   # Warstwa aplikacji (use cases, CQRS)
├── CardActions.Domain        # Warstwa domeny (model, reguły biznesowe)
├── CardActions.Infrastructure    # Implementacje interfejsów
└── CardActions.Infrastructure.Data # Dostęp do danych

tests/
├── CardActions.Unit.Tests
└── CardActions.Integration.Tests
```

### 🔑 Kluczowe cechy

- **Clean Architecture** - separacja warstw i zależności
- **CQRS** - rozdzielenie operacji odczytu i zapisu
- **Walidacja fluent** - walidacja requestów z wykorzystaniem FluentValidation
- **Lokalizacja** - wsparcie dla wielu języków (pl, en)
- **OpenAPI/Swagger** - automatyczna dokumentacja API
- **Monitoring** - OpenTelemetry, Prometheus, health checks
- **Logowanie** - ustrukturyzowane logi z wykorzystaniem Serilog
- **Testy** - unit testy i testy integracyjne
- **CI/CD** - automatyczny pipeline z semantic versioning

## 🚀 Deployment

Projekt wykorzystuje:
- GitHub Actions do CI/CD
- Semantic versioning dla wersjonowania
- Docker i GitHub Container Registry
- Kubernetes do deploymentu

### Wersjonowanie

- `main` -> wersje produkcyjne (np. 1.2.3)
- `develop` -> wersje beta (np. 1.2.3-beta.1)

## 🛠️ Development

```bash
# Uruchomienie lokalnie
dotnet restore
dotnet build
dotnet run --project src/CardActions.Api

# Testy
dotnet test

# Docker
docker build -t cardactions-api .
docker run -p 8080:80 cardactions-api
```

## 📝 Konwencje commitów

Projekt używa conventional commits do automatycznego wersjonowania:

- `feat: ` - nowa funkcjonalność (minor)
- `fix: ` - naprawa błędu (patch)
- `BREAKING CHANGE: ` - niekompatybilna zmiana (major)
- `chore: ` - zmiany w build/tooling
- `docs: ` - zmiany w dokumentacji
- `refactor: ` - refaktoryzacja kodu
- `test: ` - zmiany w testach

## 📚 API Endpoints

- `GET /api/users/{userId}/cards/{cardNumber}/actions` - pobierz dozwolone akcje dla karty
- `GET /health` - health check
- `GET /metrics` - metryki Prometheus
- `/docs` - dokumentacja API (ReDoc)
- `/swagger` - Swagger UI
