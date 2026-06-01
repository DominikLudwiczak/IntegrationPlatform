# Uruchomienie aplikacji

## Wymagania wstępne

Przed uruchomieniem aplikacji należy utworzyć kopię pliku:

```text
IntegrationPlatform/appsettings.json
```

i zapisać ją pod nazwą:

```text
IntegrationPlatform/appsettings.Development.json
```

Następnie w nowo utworzonym pliku należy uzupełnić wartość pola `MediatRKey` znajdującego się w sekcji `Application`.

Przykład:

```json
{
  "Application": {
    "MediatRKey": "YOUR_KEY_HERE"
  }
}
```

## Uzyskanie klucza MediatR

1. Przejdź na stronę https://mediatr.io/#pricing.
2. Wybierz darmowy plan (Free).
3. Załóż konto lub zaloguj się.
4. Poproś o wygenerowanie darmowego klucza licencyjnego.
5. Po otrzymaniu klucza skopiuj go i wklej do pola `MediatRKey` w pliku `appsettings.Development.json`.

## Uruchomienie aplikacji

W głównym katalogu projektu uruchom polecenie:

```bash
docker compose up -d
```

Następnie poczekaj na:

* zbudowanie obrazów Docker,
* uruchomienie wszystkich kontenerów.

## Dostęp do aplikacji

Po poprawnym uruchomieniu środowiska dostępne będą następujące adresy:

### Główne API

```text
http://localhost:5000/api/swagger/index.html
```

### Konsument głównego API

```text
http://localhost:5001/swagger/index.html
```

## Zatrzymanie środowiska

Aby zatrzymać wszystkie kontenery, wykonaj:

```bash
docker compose down
```

# Awaryjny sposób uruchomienia aplikacji

Jeżeli uruchomienie aplikacji przy użyciu standardowej procedury zakończy się niepowodzeniem, można skorzystać z poniższej metody awaryjnej.

## Wymagania wstępne

1. Usuń wcześniej utworzone kontenery Docker związane z projektem.
2. Otwórz plik `docker-compose.yml`.
3. Zakomentuj sekcje:

   * `integration_platform`,
   * `consumer`.

## Uruchomienie środowiska

W głównym katalogu projektu uruchom polecenie:

```bash
docker compose up -d
```

Poczekaj na:

* zbudowanie wymaganych obrazów Docker,
* uruchomienie wszystkich pozostałych kontenerów.

## Uruchomienie aplikacji głównej

Po uruchomieniu środowiska przejdź do głównego katalogu aplikacji i wykonaj polecenie:

```bash
dotnet run
```

## Uruchomienie aplikacji Consumer

Otwórz nową konsolę, pozostawiając poprzednią uruchomioną. Następnie przejdź do katalogu `Consumer` i wykonaj polecenie:

```bash
dotnet run
```

## Dostęp do aplikacji

Po poprawnym uruchomieniu obu aplikacji adresy dostępu pozostają takie same jak w przypadku standardowego sposobu uruchomienia:

### Główne API

```text
http://localhost:5000/api/swagger/index.html
```

### Konsument głównego API

```text
http://localhost:5001/swagger/index.html
```


### Uwagi

W celu zasymulowania rzeczywistych warunków wykonywania operacji zastosowałem kontrolowane opóźnienia procesu. W trakcie działania poszczególnych operacji wartość postępu (progress) jest aktualizowana w sposób sztuczny, aby odzwierciedlić stopniowe wykonywanie zadania.
Dodatkowo w wybranych scenariuszach zaimplementowałem losowe generowanie wyjątków oraz błędów wykonania. Pozwoliło mi to przetestować mechanizmy obsługi błędów, raportowania stanu operacji oraz zachowanie aplikacji w sytuacjach awaryjnych.