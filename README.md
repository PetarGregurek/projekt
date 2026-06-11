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

Test podaci za prijavu

Admin:

Email: admin@boardgamereviews.local
Username: admin
Password: Admin123!
User:

Email: user@boardgamereviews.local
Username: user
Password: User123!

## Google OAuth (3rd party login)

Google login je podrzan preko ASP.NET Core Identity external login toka.

Preduvjeti:

1. Pokreni aplikaciju preko HTTPS profila (`https://localhost:7103`).
2. U Google Cloud Console kreiraj OAuth Client (`Web application`).
3. Dodaj redirect URI: `https://localhost:7103/signin-google`.
4. Preuzmi `ClientId` i `ClientSecret`.

Spremanje tajni u developmentu (bez hardkodiranja):

```powershell
cd BoardGameReviews
dotnet user-secrets init
dotnet user-secrets set "Authentication:Google:ClientId" "<GOOGLE_CLIENT_ID>"
dotnet user-secrets set "Authentication:Google:ClientSecret" "<GOOGLE_CLIENT_SECRET>"
```

Nakon toga na Login stranici pojavljuje se gumb `Login with Google`.

Sigurnosna napomena:

- Ne spremati `ClientSecret` u `appsettings.json` ili u Git repozitorij.
- U produkciji koristiti tajne iz sigurnog secret storea / environment varijabli.