# us-mecanic — conventions

Application de suivi du parc de véhicules (maintenance, problèmes, coûts, certification de trajet). Voir README.md pour la structure, la CI/CD et le déploiement.

## Règles générales
- Code et commentaires en français pour le métier, identifiants de code en anglais.
- Fichiers courts et focalisés : ESLint `max-lines` à 300 côté front. Côté API, une classe = une responsabilité.
- Commits au format Conventional Commits (`feat:`, `fix:`, `refactor:`, `chore:`…) : ils pilotent la version et le CHANGELOG (release-please). Ne jamais modifier la version à la main.
- Avant de pousser : `npm run lint && npm run typecheck && npm test` dans `web/`, et `dotnet format --verify-no-changes && dotnet test` dans `api/`.

## Front (`web/`)
- Organisation cible par domaine : `src/features/<domaine>/{api,components,hooks,pages,types}`, et `src/shared/` pour le générique. Le code historique (`components/`, `services/`, `hooks/`) est migré au fil de l'eau.
- Les appels HTTP passent par TanStack Query. Les query keys incluent tous les paramètres (ex. `["geoloc", immat, tripId]`).
- Aucune URL ni configuration d'environnement en dur : passer par `src/config/appConfig.ts`.
- La géoloc / certification de trajet (`#/geoloc/...`, `#/embed/geoloc/...`) doit rester accessible aux mêmes URL. Elle consomme l'API géoloc existante via `geoClient`.

## API (`api/`)
- Architecture hexagonale : Domain ← Application ← Infrastructure / Api. Le Domain ne dépend de rien.
- Les sources externes (km, infos véhicule…) sont des ports dans Application et des adaptateurs dans Infrastructure. Le front ne connaît que l'API.
- Erreurs métier attendues : `Result<T>` + `DomainError`, pas d'exception. Réponses d'erreur HTTP en ProblemDetails.
- Tous les endpoints sont authentifiés par défaut (fallback policy Keycloak). Un endpoint public doit le déclarer avec `AllowAnonymous()`.
- Warnings = erreurs (`TreatWarningsAsErrors`). Versions NuGet centralisées dans `Directory.Packages.props`.
