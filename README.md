# us-mecanic

Suivi du parc de véhicules Urgence Santé : maintenance (km, assurance, contrôle technique), suivi des problèmes et de leur résolution, suivi économique, ainsi que la certification de trajet par géolocalisation.

## Structure

```
web/      Front React 19 + Vite + MUI + TanStack Query (auth Keycloak)
api/      API .NET 10 — architecture hexagonale
  src/UsMecanic.Domain          entités, règles métier, value objects
  src/UsMecanic.Application     cas d'usage, ports (interfaces)
  src/UsMecanic.Infrastructure  adaptateurs : persistance, sources externes (km, télématique…)
  src/UsMecanic.Api             endpoints HTTP, auth, OpenAPI
  tests/                        tests unitaires et d'intégration
deploy/   docker compose + script de redéploiement (dev / prod)
```

Le front ne parle qu'à l'API `us-mecanic`. L'agrégation des sources externes (kilométrage, infos véhicule…) se fait côté API derrière des ports de la couche Application. Déléguer une source à un autre service revient à changer d'adaptateur, sans impact sur le front.

## Développement local

```bash
# Front — http://localhost:5173
cd web
npm ci
npm run dev
npm run lint && npm run typecheck && npm test

# Base PostgreSQL locale (identifiants de appsettings.Development.json)
docker run -d --name us-mecanic-db -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres:16

# API — http://localhost:5255/scalar (doc OpenAPI en Development)
# Les migrations EF Core sont appliquées au démarrage (Database:MigrateOnStartup, true par défaut).
cd api
dotnet run --project src/UsMecanic.Api
dotnet test UsMecanic.slnx
dotnet format UsMecanic.slnx
```

Configuration locale du front : copier `web/.env.example` en `web/.env.local`.

Les tests d'intégration de l'API créent une base jetable sur le PostgreSQL local
(ou celui de `TEST_POSTGRES_CONNECTION`). Nouvelle migration :

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add <Nom> --project src/UsMecanic.Infrastructure --output-dir Persistence/Migrations
```

Feuille de route : [docs/ROADMAP.md](docs/ROADMAP.md).

## Import du parc existant

Écran **Parc → Importer** (ou `POST /api/vehicles/import?dryRun=true|false`, fichier en multipart).
Fichier CSV avec en-tête, séparateur `;` ou `,`, UTF-8 ou Windows-1252 (export Excel) :
seule la colonne `Immatriculation` est obligatoire ; `Marque`, `Modèle`, `Mise en circulation`
(JJ/MM/AAAA), `VIN` et `Indicatif` sont facultatives. Les véhicules sont rapprochés par immatriculation :
un véhicule connu est mis à jour, une cellule vide n'efface pas la valeur existante.
Modèle : [docs/modele-import-vehicules.csv](docs/modele-import-vehicules.csv).

## Configuration du front au runtime

Une même image est déployée en dev et en prod. Le conteneur génère `/config.js` au démarrage à partir de ces variables (vide = valeur par défaut, cf. `web/src/config/appConfig.ts`) :

| Variable conteneur | Variable locale (`.env.local`) | Rôle |
|---|---|---|
| `APP_ENV_NAME` | `VITE_ENV_NAME` | Nom affiché (dev, prod…) |
| `APP_API_URL` | `VITE_API_URL` | URL de l'API us-mecanic |
| `APP_LEGACY_API_URL` | `VITE_LEGACY_API_URL` | Ancienne API (logs, analyses, notifications) en attendant leur reprise |
| `APP_GEOLOC_API_URL` | `VITE_GEOLOC_API_URL` | API géoloc (certification de trajet) |
| `APP_MAP_STYLE_URL` | `VITE_MAP_STYLE_URL` | Style de carte MapLibre |
| `APP_KEYCLOAK_URL` / `_REALM` / `_CLIENT_ID` | `VITE_KEYCLOAK_*` | Keycloak |

## Branches, versions et CI/CD

Une seule branche longue : `master`. Plus de branche `dev`.

- Tout changement passe par une **PR vers `master`** (CI verte obligatoire).
- Merge sur `master` : CI, puis publication des images `dev`, `latest-dev` et `dev-<sha>` (environnement **dev**).
- Release (PR release-please fusionnée) : tag `vX.Y.Z`, puis images `X.Y.Z`, `X.Y` et `latest` (environnement **prod**).

- **Versionnage `major.minor.patch`** via [release-please](https://github.com/googleapis/release-please) à partir des [Conventional Commits](https://www.conventionalcommits.org/fr/) :
  - `fix:` → patch ;
  - `feat:` → minor ;
  - `feat!:` ou `BREAKING CHANGE:` → major (tant qu'on est en `0.x`, un breaking change bumpe le minor).
- À chaque push sur `master`, release-please ouvre ou met à jour une **PR de release** (numéro de version + `CHANGELOG.md`). La fusionner crée le tag, la release GitHub et publie les images.
- La version est synchronisée dans `web/package.json` et `api/Directory.Build.props`. Elle ne se modifie pas à la main.

Workflows (`.github/workflows/`) :

- `ci.yml` : lint, typecheck, tests + couverture et build pour le web et l'API (sur chaque PR, et réutilisé par les deux suivants) ;
- `master.yml` : push sur `master` → CI → publication des images dev ;
- `release.yml` : push sur `master` → release-please → publication des images versionnées ;
- `docker-publish.yml` : workflow réutilisable de build et push Docker Hub.

Images Docker Hub : `alexandredelesse/us-mecanic` (web) et `alexandredelesse/us-mecanic-api`.

Secrets GitHub requis : `DOCKERHUB_USERNAME`, `DOCKERHUB_TOKEN`.

## Déploiement

Watchtower, sur le VPS, suit les tags (configuration par le propriétaire, hors dépôt) :

| Environnement | Tag suivi |
|---|---|
| dev | `latest-dev` (republié à chaque merge sur `master`) |
| prod | une version figée (`X.Y.Z`) ou `latest` |

`deploy/redeploy.sh` reste utilisable pour un déploiement manuel ou un retour arrière (tag précis, ex. `./redeploy.sh prod 0.2.0` ou `./redeploy.sh dev dev-<sha>`).

Sur le serveur, dans le dossier `deploy/` :

```bash
cp .env.dev.example .env.dev      # une seule fois, puis adapter
./redeploy.sh dev                 # image "latest-dev" (dernier merge sur master)
./redeploy.sh prod 0.2.1          # version précise en prod
```

| Environnement | Web | API |
|---|---|---|
| dev | 3020 | 3030 |
| prod | 3022 | 3032 |

Les URL du front, y compris la géoloc intégrée `#/embed/geoloc/:immat/:tripId`, restent sur les mêmes ports qu'avant.

Chaque environnement a sa propre base PostgreSQL (conteneur `db`, non exposé, données dans le volume
`us-mecanic-<env>_db-data`). Renseigner `DB_PASSWORD` dans `.env.<env>` avant le premier démarrage,
ainsi que `APP_API_URL` (URL publique de l'API) et `API_CORS_ORIGIN` (URL du front) pour que le front
puisse joindre l'API.
