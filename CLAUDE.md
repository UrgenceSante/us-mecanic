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

## Équipe d'agents
Le projet est piloté avec des agents spécialisés (`.claude/agents/`) et des commandes (`.claude/commands/`). Mode d'emploi : `docs/GUIDE-agents.md`.
- `product` spécifie (`docs/specs/`, `docs/ROADMAP.md`, `docs/decisions/`), `pm` gère milestones, issues et avancement, `dev` implémente, `tester` teste, `reviewer` relit en lecture seule, `docs` documente.
- Commandes : `/spec`, `/planifier`, `/tache <n>`, `/statut`.
- Un milestone GitHub = une phase de `docs/ROADMAP.md`. Une issue = une demi-journée de travail = une branche `feat/<n>-slug` = une PR avec `Closes #<n>`.
- Jamais de push direct sur `master`. `dev` coche la ligne de roadmap qu'il livre, dans sa PR.
- Merge automatique par l'orchestrateur (jamais par `dev`, `tester` ou `reviewer`), via l'auto-merge GitHub, si : CI verte, verdict `reviewer` APPROUVÉ, critères de la PR cochés et aucune question métier en attente.
- Accord explicite du propriétaire requis, sans auto-merge, pour une PR qui : modifie ou supprime des données par migration (une création de table reste automatique) ; touche à l'authentification ou à la sécurité ; touche au déploiement (Dockerfile, `deploy/`, `.github/workflows/`) ; ou porte un choix métier signalé en relecture.
- Un agent ne sort pas du périmètre de l'issue : toute découverte hors sujet est signalée, `pm` crée l'issue.
- Positions GPS et identités des conducteurs : données personnelles, jamais dans les logs.
- GitHub Project : `PROJECT_NUMBER=2` (https://github.com/orgs/UrgenceSante/projects/2)
