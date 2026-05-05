# ── Build stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY BoardGameReviews.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app/publish --no-restore

# ── Runtime stage ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install EF tools for database update at startup
COPY --from=build /root/.dotnet /root/.dotnet
COPY --from=build /src /src
COPY --from=build /app/publish ./

ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet tool install --global dotnet-ef --version 8.0.0

EXPOSE 8080

# Run migrations then start the app
ENTRYPOINT ["sh", "-c", "cd /src && dotnet ef database update && cd /app && dotnet BoardGameReviews.dll"]
