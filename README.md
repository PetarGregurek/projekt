# BoardGameReviews

ASP.NET Core 8 MVC aplikacija za recenzije društvenih igara s EF Core + SQL Server backendom.

---

## Preduvjeti

| Alat | Verzija |
|---|---|
| .NET SDK | 8.0+ |
| SQL Server (LocalDB) | 2019+ / mssqllocaldb |
| Docker Desktop | 4.x+ (samo za Docker način) |

---

## Načini pokretanja

### 1. Lokalni MSSQL (preporučeno)

Koristi SQL Server LocalDB koji dolazi s Visual Studio / SQL Server Express instalacijom.

```powershell
# Provjera / pokretanje LocalDB instance
sqllocaldb start MSSQLLocalDB

# Pokretanje aplikacije
cd BoardGameReviews
dotnet run
```

Aplikacija je dostupna na: `http://localhost:5252`

Baza `BoardGameReviews` kreira se automatski pri prvom pokretanju putem EF migracija (ako ne postoji).  
Connection string se čita iz `appsettings.json`:

```
Server=(localdb)\mssqllocaldb;Database=BoardGameReviews;Trusted_Connection=True
```

---

### 2. Docker (SQL Server + App)

Pokreće i SQL Server i aplikaciju u containerima. Ne zahtijeva lokalnu instalaciju SQL Servera.

```powershell
# Iz root direktorija projekta (gdje se nalazi docker-compose.yml)
cd C:\...\projekt

docker compose up --build
```

Aplikacija je dostupna na: `http://localhost:8080`

Pri svakom pokretanju app container automatski primjenjuje sve EF migracije na bazu.

#### Zaustavljanje

```powershell
docker compose down          # zaustavi containere, zadrži podatke
docker compose down -v       # zaustavi i obriši volume (briše bazu!)
```

#### Lozinka za SA account

Lozinka se čita iz `.env` fajla u root direktoriju projekta:

```
MSSQL_SA_PASSWORD=DevPass!23
```

`.env` je dodan u `.gitignore` — promijeni lozinku po potrebi, nikad je ne committaj.

---

## Migracije

Ako dodaš promjenu na modelu, kreiraj novu migraciju:

```powershell
cd BoardGameReviews
dotnet ef migrations add ImeMigracije
dotnet ef database update
```

Za Docker: dovoljno je samo `docker compose up --build` jer se `database update` pokreće automatski.

---

## Struktura projekta

```
projekt/
├── BoardGameReviews/
│   ├── Controllers/        # GameController, HomeController
│   ├── Data/               # AppDbContext, IBoardGameRepository, EfBoardGameRepository
│   ├── Migrations/         # EF migracije
│   ├── Models/             # Game, Review, User, Event, Category, GameType, Publisher
│   ├── Views/              # Razor pogledi
│   ├── appsettings.json              # LocalDB connection string (primary)
│   ├── appsettings.Development.json  # Dev logging overrides
│   ├── appsettings.Docker.json       # Docker connection string override
│   └── Dockerfile
├── docker-compose.yml
├── .env                    # SA lozinka (nije u gitu)
└── projekt.sln
```