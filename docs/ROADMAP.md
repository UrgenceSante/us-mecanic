# Feuille de route — gestion de flotte

Objectif : faire de us-mecanic l'application de gestion du parc de véhicules Urgence Santé
(véhicules, kilométrage et échéances, problèmes et résolution, coûts), en reprenant
progressivement les fonctions de l'ancienne API.

Principes retenus :

- **Base PostgreSQL**, propre à us-mecanic.
- **Aucune migration automatique des données de l'ancienne API** : les fonctions sont reprises
  dans la nouvelle API ; l'intérêt de migrer des données sera évalué au cas par cas.
- **Intégration de l'existant** par import (fichier aujourd'hui, autres sources demain),
  derrière un port de la couche Application.
- La géoloc / certification de trajet reste aux mêmes URL et garde son API dédiée.

## Phase 0 — Socle ✅

- [x] Front déplacé dans `web/`, configuration runtime (une image pour dev et prod)
- [x] API .NET 10 hexagonale, auth Keycloak par défaut, ProblemDetails
- [x] CI (lint, typage, tests, build), images Docker Hub, release-please
- [x] Déploiement docker compose (dev / prod)

## Phase 1 — Véhicules ✅

- [x] Persistance PostgreSQL (EF Core, migrations appliquées au démarrage)
- [x] Véhicule : immatriculation (unique), marque, modèle, mise en circulation, VIN, n° interne
- [x] Statut : en service, immobilisé, sorti du parc (définitif)
- [x] Endpoints `/api/vehicles` (liste filtrée, fiche, création, modification, statut)
- [x] Import CSV de l'existant : simulation puis confirmation, rapprochement par immatriculation,
      bilan ligne par ligne (`docs/modele-import-vehicules.csv`)
- [x] Front `features/vehicles` : liste du parc, fiche, formulaire, import
- [ ] Rôles Keycloak (lecture seule / gestionnaire) — à définir

## Phase 2 — Kilométrage et échéances

- [ ] Relevés kilométriques (saisie manuelle, historique, contrôle de cohérence)
- [ ] Port « source de kilométrage » pour une source externe (télématique…)
- [ ] Échéances : assurance, contrôle technique, entretien (au km et/ou à la date)
- [ ] Tableau de bord des échéances proches ou dépassées
- [ ] Import de l'historique (km, échéances) sur le modèle de l'import véhicules

## Phase 3 — Problèmes et résolution

Reprise de ce que fait l'ancienne API (logs mécaniques, analyses, actions, référentiels).

- [ ] Signalements de problème rattachés au véhicule
- [ ] Analyse : nature, élément concerné, immobilisation (lien avec le statut du véhicule)
- [ ] Actions de résolution : acteur, type, contrainte, échéance, clôture
- [ ] Référentiels (natures, acteurs, actions, contraintes) administrables
- [ ] Migration des écrans `MecanicLog` / `Analyse` vers `features/issues`

## Phase 4 — Coûts

- [ ] Dépenses rattachées au véhicule (entretien, réparation, assurance…), liées aux actions
- [ ] Synthèse par véhicule et par période, coût au kilomètre

## Phase 5 — Notifications et nettoyage

- [ ] Notifications (abonnements push) reprises dans la nouvelle API
- [ ] Suppression du code front historique (`components/`, `services/`, `hooks/`)
- [ ] Retrait de `APP_LEGACY_API_URL` et arrêt de l'ancienne API
