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

# API — http://localhost:5255/scalar (doc OpenAPI en Development)
cd api
dotnet run --project src/UsMecanic.Api
dotnet test UsMecanic.slnx
dotnet format UsMecanic.slnx
```

Configuration locale du front : copier `web/.env.example` en `web/.env.local`.

## Configuration du front au runtime

Une même image est déployée en dev et en prod. Le conteneur génère `/config.js` au démarrage à partir de ces variables (vide = valeur par défaut, cf. `web/src/config/appConfig.ts`) :

| Variable conteneur | Variable locale (`.env.local`) | Rôle |
|---|---|---|
| `APP_ENV_NAME` | `VITE_ENV_NAME` | Nom affiché (dev, prod…) |
| `APP_API_URL` | `VITE_API_URL` | URL de l'API |
| `APP_GEOLOC_API_URL` | `VITE_GEOLOC_API_URL` | API géoloc (certification de trajet) |
| `APP_MAP_STYLE_URL` | `VITE_MAP_STYLE_URL` | Style de carte MapLibre |
| `APP_KEYCLOAK_URL` / `_REALM` / `_CLIENT_ID` | `VITE_KEYCLOAK_*` | Keycloak |

## Branches, versions et CI/CD

| Branche | Rôle | Règles | Publication |
|---|---|---|---|
| `dev` | intégration, environnement **dev** | push direct autorisé | à chaque push : CI puis images `dev` et `dev-<sha>` |
| `master` | **prod** | PR depuis `dev` uniquement, CI verte obligatoire | release-please → tag `vX.Y.Z` → images `X.Y.Z`, `X.Y`, `latest` |

- **Versionnage `major.minor.patch`** via [release-please](https://github.com/googleapis/release-please) à partir des [Conventional Commits](https://www.conventionalcommits.org/fr/) :
  - `fix:` → patch ;
  - `feat:` → minor ;
  - `feat!:` ou `BREAKING CHANGE:` → major (tant qu'on est en `0.x`, un breaking change bumpe le minor).
- À chaque push sur `master`, release-please ouvre ou met à jour une **PR de release** (numéro de version + `CHANGELOG.md`). La fusionner crée le tag, la release GitHub et publie les images.
- La version est synchronisée dans `web/package.json` et `api/Directory.Build.props`. Elle ne se modifie pas à la main.
- Fusionner `dev` → `master` avec un **merge commit** (pas de squash) pour conserver les messages de commit dans le changelog.

Workflows (`.github/workflows/`) :

- `ci.yml` : lint, typecheck, tests + couverture et build pour le web et l'API (sur chaque PR, et réutilisé par les deux suivants) ;
- `dev.yml` : push sur `dev` → CI → publication des images dev ;
- `release.yml` : push sur `master` → release-please → publication des images versionnées ;
- `docker-publish.yml` : workflow réutilisable de build et push Docker Hub.

Images Docker Hub : `alexandredelesse/us-mecanic` (web) et `alexandredelesse/us-mecanic-api`.

Secrets GitHub requis : `DOCKERHUB_USERNAME`, `DOCKERHUB_TOKEN`.

## Déploiement

Sur le serveur, dans le dossier `deploy/` :

```bash
cp .env.dev.example .env.dev      # une seule fois, puis adapter
./redeploy.sh dev                 # image "dev" la plus récente
./redeploy.sh prod 0.2.1          # version précise en prod
```

| Environnement | Web | API |
|---|---|---|
| dev | 3020 | 3030 |
| prod | 3022 | 3032 |

Les URL du front, y compris la géoloc intégrée `#/embed/geoloc/:immat/:tripId`, restent sur les mêmes ports qu'avant.
