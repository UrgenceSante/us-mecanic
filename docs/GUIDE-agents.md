# Guide - travailler avec l'équipe d'agents

## Principe

Tu n'interviens qu'à 3 moments : valider la spec d'une phase, valider le découpage en tâches, relire et merger les PR. Entre les deux, `dev` -> `tester` -> `reviewer` tournent seuls sur une issue.

L'avancement vient de GitHub : milestone = phase de `docs/ROADMAP.md`, issue = tâche d'une demi-journée, pourcentage = issues fermées. Rien n'est déclaratif.

## Mise en place (une fois)

1. Récupère cette branche ou merge la PR qui l'ajoute, puis `git pull` sur `master`.
2. Autorise la CLI GitHub à gérer les Projects : `gh auth refresh -s project`.
3. Lance `claude` à la racine du dépôt et colle le prompt de démarrage ci-dessous.
4. Quand `/planifier` a créé le GitHub Project, ouvre-le sur github.com :
   - vue Board : ajoute une colonne « In review » au champ Status ;
   - Settings > Workflows : active « Item closed » -> Done et « Pull request merged » -> Done ;
   - affiche le champ Milestone dans la vue.
   Si tu n'as pas le droit de créer un Project dans l'organisation, crée-le sous ton compte et donne son numéro à `pm`.
5. Note le numéro du Project dans la section « Équipe d'agents » de `CLAUDE.md`.

## Prompt de démarrage

```
Nous mettons en place l'équipe d'agents sur us-mecanic, qui existe déjà. Lis CLAUDE.md, README.md, docs/ROADMAP.md et .claude/.

Étape 1 - État des lieux, sans rien modifier : avec l'agent pm, liste les issues, les PR ouvertes, les milestones et les branches distantes qui existent déjà. Compare docs/ROADMAP.md au code : dis-moi si les phases cochées sont vraiment livrées et signale les écarts. Vérifie aussi `gh auth status` (scope `project`).

Étape 2 - Lance /spec sur la première phase non terminée de la roadmap. Interviewe-moi par lots de 3 à 5 questions, sans me redemander ce qui est dans la roadmap ou le code.

Étape 3 - Quand je dis « spec validée » : lance /planifier pour cette phase. Propose le découpage, attends mon accord, puis crée milestone, issues et GitHub Project.

Étape 4 - Termine par /statut.

Règles : aucun code applicatif à ce stade. Rien d'écrit sur GitHub avant mon accord explicite.
```

## Routine

| Quand | Commande | Ce qui se passe |
|---|---|---|
| Début de journée | `/statut` | `pm` calcule l'avancement depuis GitHub et te dit quoi lancer |
| Pour avancer | `/tache 12` | dev -> tester -> reviewer -> PR ouverte, puis attend ton merge |
| Tu relis la PR | sur GitHub | tu merges ou tu commentes ; au merge l'issue passe à Done |
| Nouvelle phase ou besoin | `/spec <sujet>` puis `/planifier` | spec mise à jour, puis nouvelles issues |
| Fin de phase | « utilise l'agent docs pour mettre à jour README et doc d'exploitation » | documentation alignée sur le code |

Au début, lance `/tache` sur une seule issue à la fois pour juger la qualité. Ensuite, tu peux paralléliser les issues indépendantes dans des sessions séparées, une branche chacune.

## Où lire l'avancement

- GitHub > Milestones : pourcentage par phase.
- GitHub Project (Board) : état de chaque tâche.
- `/statut` : la synthèse en 10 lignes.

## Si ça se passe mal

- Une issue tourne en rond : `/tache` s'arrête après 3 boucles. Précise les critères « fait » ou découpe l'issue.
- Le résultat ne correspond pas à ce que tu voulais : corrige la spec ou l'issue, pas le code à la main, sinon la prochaine itération refait l'erreur.
- Un agent sort de son rôle : resserre son champ `tools` ou sa section « Interdits » dans `.claude/agents/`.
- release-please : le titre des PR et les commits doivent rester en Conventional Commits, les agents ne touchent jamais à la version.
