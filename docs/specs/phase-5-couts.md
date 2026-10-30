# Phase 5 — Coûts

Statut : en cours de cadrage (2026-10-30)

## Contexte et objectif

Suivre ce que coûte chaque véhicule et le parc : entretien, réparations, carburant, péages, emprunts.
Principe retenu : **rester simple** (un montant, sans détail pièces / main d'œuvre ni distinction HT / TTC),
et affiner à l'usage.

Une grande partie des coûts dépend de **sources externes** : cartes carburant Total (API), télépéage Ulys
(API ou carte Total), suivi des emprunts. Conformément à l'architecture, chaque source est un port
(Application) avec un adaptateur (Infrastructure).

## Utilisateurs et rôles

- **Administrateur du parc** : saisit et consulte les coûts.
- Le suivi des emprunts n'est pas accessible à tous les utilisateurs (droits à préciser, rôles en phase 9).

## User stories

À compléter.

## Règles métier

- Montant unique par dépense, sans détail ni distinction HT / TTC (à affiner plus tard).
- Pas de répartition des coûts récurrents : chaque dépense est saisie à sa date.

## Hors périmètre

- Détail des factures (pièces, main d'œuvre), TVA.

## Impact sur le modèle de données

À compléter.

## Questions ouvertes

- Accès aux API Total et Ulys (contrats, documentation).
- Périmètre exact du suivi des emprunts et de sa visibilité.
