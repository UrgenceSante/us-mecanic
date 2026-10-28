# Phase 3 — Carnet d'entretien

Statut : brouillon (cadrage en cours)

## Contexte et objectif

La phase 2 suit les échéances et enregistre leurs réalisations (date, km, commentaire). La phase 3
ajoute le carnet d'entretien : l'historique des interventions réalisées sur chaque véhicule, prévues
(liées à une ou plusieurs échéances) ou imprévues (réparation sans échéance).

Principe retenu : **rester simple**. Le détail (coûts, pièces jointes, durée d'immobilisation…)
sera ajouté plus tard selon le besoin réel.

## Utilisateurs et rôles

- **Administrateur du parc** : seul profil (pas d'authentification avant la phase 9).

## User stories

À préciser. Périmètre retenu :

- Saisir une intervention : date, km, garage, travaux.
- Une intervention peut réaliser **plusieurs** échéances du véhicule à la fois.
- Une intervention peut n'être liée à **aucune** échéance (réparation imprévue).
- Garages : liste administrable ; le champ de saisie permet de créer un garage s'il n'existe pas.

## Règles métier

À préciser.

## Hors périmètre

- Coûts : phase 5.
- Pièces jointes (factures) : phase 6.
- Durée d'immobilisation, intervenants, pièces : plus tard, selon le besoin.

## Impact sur le modèle de données

À préciser.

## Questions ouvertes

- Relation avec les « réalisations » d'échéance existantes (phase 2).
- Modification et suppression d'une intervention.
