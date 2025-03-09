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
├── CardActions.Integration.Tests
└── CardActions.Architecture.Tests
```

### 🔑 Kluczowe cechy

- **Clean Architecture** - separacja warstw i zależności
- **Domain-Driven Design** - implementacja wzorców DDD:
    - Value Objects (CardAction, CardActionRule)
    - Domain Services (ICardActionService)
    - Domain Policies (CardActionPolicy)
    - Bounded Context (Card Actions)
- **CQRS** - rozdzielenie operacji odczytu i zapisu
- **Walidacja fluent** - walidacja requestów z wykorzystaniem FluentValidation
- **Lokalizacja** - wsparcie dla wielu języków (pl, en)
- **OpenAPI/Swagger** - automatyczna dokumentacja API
- **Monitoring** - OpenTelemetry, Prometheus, health checks
- **Logowanie** - ustrukturyzowane logi z wykorzystaniem Serilog
- **Testy** - unit testy, testy integracyjne i testy architektury
- **CI/CD** - automatyczny pipeline z semantic versioning

### 💼 Komponenty biznesowe

System składa się z następujących kluczowych komponentów:

1. **Handler akcji karty** - obsługuje zapytania o dozwolone akcje:
    - Walidacja danych wejściowych
    - Weryfikacja istnienia karty
    - Określanie dostępnych akcji

2. **Polityka akcji** - definiuje reguły biznesowe:
    - Weryfikacja warunków dla akcji
    - Sprawdzanie wymagań PIN-u
    - Zarządzanie regułami dostępu

3. **Provider reguł** - zarządza konfiguracją akcji:
    - Wczytywanie reguł z pliku CSV
    - Przechowywanie reguł w pamięci
    - Monitorowanie zmian reguł

4. **Walidacja żądań** - zapewnia poprawność danych:
    - Równoległa walidacja przez wiele walidatorów
    - Wczesne wykrywanie błędów
    - Spójne komunikaty błędów

## 🚀 Deployment

Projekt wykorzystuje:

- GitHub Actions do CI/CD
- Semantic versioning dla wersjonowania
- Docker i GitHub Container Registry

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

## 🧪 Testy

Projekt zawiera trzy rodzaje testów:

### Unit Testy

Testy jednostkowe sprawdzające logikę biznesową i komponenty w izolacji.

### Testy Integracyjne

Testy weryfikujące współpracę między komponentami i integrację z zewnętrznymi systemami.

### Testy Architektury

Automatyczne testy weryfikujące zgodność z założeniami architektonicznymi:

- Poprawność zależności między warstwami
- Zgodność z konwencjami nazewniczymi
- Prawidłowe użycie wzorców (CQRS, Clean Architecture)
- Spójność konfiguracji projektów
- Weryfikacja wersji pakietów NuGet
