# Lokalne testowanie GitHub Actions

## Wymagania
- [act](https://github.com/nektos/act) - narzędzie do lokalnego testowania GitHub Actions
- Docker

## Instalacja act
```bash
# macOS
brew install act

# Linux
curl -s https://raw.githubusercontent.com/nektos/act/master/install.sh | sudo bash
```

## Testowanie workflowów

### Pull Request Workflow
Testowanie workflow dla pull requestów:
```bash
# Dry run (bez wykonywania rzeczywistych akcji)
act pull_request -W .github/workflows/pr-workflow.yml -n --container-architecture linux/amd64

# Pełne wykonanie
act pull_request -W .github/workflows/pr-workflow.yml --container-architecture linux/amd64
```

### Release Workflow
Testowanie workflow dla release:
```bash
# Dry run
act push -W .github/workflows/release.yml -n --container-architecture linux/amd64

# Pełne wykonanie
act push -W .github/workflows/release.yml --container-architecture linux/amd64
```

### CI/CD Pipeline
Testowanie głównego pipeline:
```bash
# Dry run
act push -W .github/workflows/ci-cd.yml -n --container-architecture linux/amd64

# Pełne wykonanie
act push -W .github/workflows/ci-cd.yml --container-architecture linux/amd64

# Testowanie pojedynczych jobów
act push -j build -n --container-architecture linux/amd64
act push -j release -n --container-architecture linux/amd64
act push -j docker -n --container-architecture linux/amd64
act push -j deploy-docs -n --container-architecture linux/amd64
act push -j deploy-demo -n --container-architecture linux/amd64
```

## Ważne uwagi
1. Parametr `--container-architecture linux/amd64` jest wymagany dla komputerów z procesorami Apple Silicon (M1/M2).

2. Flagi:
   - `-n` - dry run (nie wykonuje rzeczywistych akcji)
   - `-W` - ścieżka do konkretnego pliku workflow
   - `-v` - tryb verbose dla debugowania
   - `-j` - uruchomienie konkretnego joba

3. Sekrety:
   Jeśli workflow wymaga sekretów, utwórz plik `.secrets` w głównym katalogu:
   ```bash
   GITHUB_TOKEN=your_token_here
   ```
   I uruchom act z flagą `--secret-file .secrets`

4. Artefakty:
   - Artefakty są zapisywane lokalnie w katalogu `artifacts`
   - Raporty z testów będą dostępne w `artifacts/test-results`
   - Raporty pokrycia kodu w `artifacts/code-coverage-report`

## Struktura workflow
```
.github/workflows/
├── ci-cd.yml         # Główny pipeline CI/CD
├── pr-workflow.yml   # Weryfikacja Pull Requestów
└── release.yml       # Automatyczne release'y
```

## Troubleshooting
1. Jeśli pojawią się problemy z uprawnieniami Dockera:
   ```bash
   sudo chmod 666 /var/run/docker.sock
   ```

2. Jeśli pojawią się problemy z pamięcią:
   ```bash
   act -W .github/workflows/pr-workflow.yml --container-architecture linux/amd64 -P ubuntu-latest=catthehacker/ubuntu:act-latest
   ```

3. Jeśli pojawią się problemy z Node.js w semantic-release:
   ```bash
   act push -j release --container-architecture linux/amd64 -P ubuntu-latest=node:16-buster
   ```

4. Jeśli pojawią się problemy z Docker Buildx:
   ```bash
   # Przed uruchomieniem testu
   docker buildx create --use
   ``` 