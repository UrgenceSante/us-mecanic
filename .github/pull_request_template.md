Closes #

<!-- Le titre de la PR suit Conventional Commits (feat:, fix:, refactor:, chore:...) : il pilote la version via release-please. -->

## Ce que fait cette PR

En 2 ou 3 lignes.

## Critères d'acceptation

Reprendre ceux de l'issue, avec le test ou la preuve.

- [ ]

## Vérifications

- [ ] `web/` : `npm run lint && npm run typecheck && npm test`
- [ ] `api/` : `dotnet format --verify-no-changes && dotnet test`
- [ ] Ligne de `docs/ROADMAP.md` cochée si cette PR la livre
- [ ] Version et `CHANGELOG.md` non modifiés à la main

## Rapport du tester et verdict du reviewer

Résultat (VERT / ROUGE), cas limites couverts, verdict (APPROUVÉ / CHANGEMENTS DEMANDÉS).

## Points d'attention pour le merge

Migrations, variables d'environnement (`deploy/.env.*.example`), changement de comportement visible.
