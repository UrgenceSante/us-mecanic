---
description: Traite une issue de bout en bout - dev, tester, reviewer, PR - puis merge automatique si les règles de CLAUDE.md le permettent
argument-hint: <numéro de l'issue>
---

Traite l'issue GitHub #$ARGUMENTS de bout en bout. Si l'issue n'existe pas, est déjà fermée ou n'a pas de critères « fait » clairs, arrête-toi et dis-le-moi.

1. Lis l'issue (`gh issue view $ARGUMENTS`) et la spec ou la ligne de `docs/ROADMAP.md` concernée.
2. Agent `dev` : implémente sur la branche `feat/$ARGUMENTS-<slug>` (ou `fix/`), en suivant `CLAUDE.md`.
3. Agent `tester` : tests et vérification des critères d'acceptation.
4. Si le résultat est ROUGE : renvoie les échecs à `dev`, puis relance `tester`. Maximum 3 boucles ; au-delà, arrête-toi et explique-moi le blocage.
5. Agent `reviewer` : revue du diff. Si CHANGEMENTS DEMANDÉS : renvoie le bloquant à `dev`, puis retest et re-revue (maximum 2 boucles).
6. Si tout est vert et approuvé : vérifie une dernière fois les commandes d'avant-push de `CLAUDE.md`, pousse la branche et ouvre la PR (`gh pr create`) avec un titre au format Conventional Commits et `Closes #$ARGUMENTS`, en remplissant le modèle de PR avec le rapport du tester et le verdict du reviewer.
7. Agent `docs` seulement si la tâche change un comportement visible, une API ou le déploiement.
8. Termine par un rapport de 5 lignes maximum : ce qui est fait, lien de la PR, points qui demandent mon attention.

Merge : applique la règle de `CLAUDE.md`. Si la PR y est éligible, coche ses critères et active l'auto-merge GitHub (méthode merge) une fois `reviewer` APPROUVÉ ; sinon, dis-moi pourquoi et attends mon merge. Ne ferme pas l'issue à la main : elle se fermera au merge. Ne modifie jamais la version ni `CHANGELOG.md`.
