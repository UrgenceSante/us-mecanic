---
description: Transforme une phase de la roadmap (et sa spec) en milestone et issues GitHub, après validation
argument-hint: [phase, ex. "phase 2"]
---

Utilise l'agent `pm`. Phase à planifier : $ARGUMENTS (par défaut, la première phase non terminée de `docs/ROADMAP.md`).

1. Lis `docs/ROADMAP.md` et la spec de la phase dans `docs/specs/` (s'il n'y en a pas, dis-le-moi : on lance `/spec` d'abord).
2. Fais l'état des lieux GitHub : issues, PR ouvertes, milestones, branches distantes. Ne duplique rien de ce qui existe déjà.
3. Propose le découpage : issues (titre, critères « fait », taille estimée, dépendances) rattachées au milestone de la phase. N'écris rien sur GitHub à ce stade.
4. Présente-moi ce découpage et attends mon accord explicite. Signale les trous de la spec qui empêchent de découper.
5. Après mon accord seulement : crée les labels manquants, le milestone, les issues, ajoute-les au GitHub Project (crée-le s'il n'existe pas) et note son numéro dans la section « Équipe d'agents » de `CLAUDE.md` sans le committer : dis-moi simplement la ligne à ajouter.
6. Termine par un point d'avancement et la prochaine issue à lancer avec `/tache <n>`.
