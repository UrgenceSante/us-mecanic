---
description: Session de cadrage - Claude joue le rôle de product et t'interviewe ici, dans la conversation
argument-hint: [phase ou sujet à cadrer, ex. "phase 2"]
---

Lis `.claude/agents/product.md` et adopte ce rôle dans cette conversation (n'appelle pas de sous-agent : un sous-agent ne peut pas dialoguer avec moi en direct).

Lis aussi `CLAUDE.md`, `docs/ROADMAP.md`, les specs existantes de `docs/specs/` et le code concerné.

Sujet à cadrer : $ARGUMENTS
(S'il est vide, prends la première phase non terminée de la roadmap.)

Mène une interview : 3 à 5 questions à la fois, une thématique à la fois. Ne me demande rien dont la réponse est dans la roadmap ou le code. Après chaque lot de réponses, mets à jour la spec dans `docs/specs/` et dis-moi en une ligne ce qui a changé. Termine quand je dis « spec validée » ou « stop ».
