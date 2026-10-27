# Phase 2 — Kilométrage et échéances

Statut : brouillon (cadrage en cours)

## Contexte et objectif

Le parc est enregistré (phase 1). La phase 2 permet de suivre le kilométrage de chaque véhicule
et ses échéances réglementaires et d'entretien, pour ne laisser passer aucune échéance.

Décisions de cadrage déjà prises (voir `docs/ROADMAP.md`) :

- Fonctionnalités d'abord : l'import de l'historique et la source de kilométrage externe
  (géoloc, télématique) sont reportés en phase 8. En phase 2, le kilométrage est saisi à la main.
- Pas d'authentification en développement (ADR 0001) ; les rôles Keycloak sont reportés en
  phase 9. En phase 2, tout utilisateur a tous les droits.
- Le carnet d'entretien (interventions réalisées) est en phase 3.

## Utilisateurs et rôles

À préciser.

## User stories

À préciser. Périmètre retenu :

- Relevés kilométriques : saisie, historique, contrôle de cohérence.
- Kilométrage actuel sur la fiche et la liste du parc.
- Échéances : contrôle technique, assurance, entretien (au km et/ou à la date).
- Échéances propres aux ambulances (agrément ARS, matériel médical, oxygène, extincteur…).
- Échéance réalisée : calcul de la suivante.
- Tableau de bord des échéances proches ou dépassées.
- Prérequis technique : désactivation de l'authentification en développement (ADR 0001).

## Règles métier

À préciser.

## Hors périmètre

- Import de l'historique (km, échéances) et source de kilométrage externe : phase 8.
- Carnet d'entretien (interventions, garage, travaux) : phase 3.
- Notifications : phase 7.
- Rôles et droits : phase 9.

## Impact sur le modèle de données

À préciser.

## Questions ouvertes

- Qui saisit les relevés, à quelle fréquence ?
- Relevé inférieur au précédent : bloqué ou accepté avec motif ?
- Correction et suppression d'un relevé.
- Liste des types d'échéances et leur périodicité.
