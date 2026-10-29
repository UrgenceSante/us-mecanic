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
- **Ordre** : les fonctionnalités d'abord, l'alimentation de la base (imports, sources externes)
  ensuite, l'authentification (rôles Keycloak) en dernier.
- **Pas de connexion en développement** (local et environnement de dev déployé), voir
  `docs/decisions/0001-pas-d-authentification-en-dev.md`. La prod reste authentifiée.

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

## Phase 2 — Kilométrage et échéances

Spec : `docs/specs/phase-2-kilometrage-echeances.md`.

- [x] Désactivation de l'authentification en développement (ADR 0001)
- [x] Relevés kilométriques : saisie manuelle, historique, contrôle de cohérence
- [x] Kilométrage actuel affiché sur la fiche et la liste du parc
- [x] Types de véhicule (ambulance, VSL, sang…) administrables
- [x] Échéances : contrôle technique, assurance, entretien (au km et/ou à la date)
- [x] Types d'échéance administrables (dont agrément ARS, matériel médical, oxygène, extincteur…)
- [x] Échéance réalisée : calcul de la suivante
- [x] Tableau de bord (page d'accueil) : échéances proches ou dépassées, problèmes du parc
- [x] Vue « Problèmes » sur la fiche véhicule (logs de l'ancienne API, transitoire)

## Phase 3 — Carnet d'entretien

Spec : `docs/specs/phase-3-carnet-entretien.md`.

- [x] Garages administrables (création à la volée depuis la saisie)
- [x] Interventions réalisées : date, km, garage, travaux ; une ou plusieurs échéances, ou aucune
- [x] « Marquer réalisée » crée une intervention ; reprise des réalisations existantes
- [x] Carnet d'entretien sur la fiche véhicule, suppression d'une intervention

## Phase 4 — Problèmes et résolution

Reprise de ce que fait l'ancienne API (logs mécaniques, analyses, actions, référentiels).

- [ ] Signalements de problème rattachés au véhicule
- [ ] Analyse : nature, élément concerné, immobilisation (lien avec le statut du véhicule)
- [ ] Actions de résolution : acteur, type, contrainte, échéance, clôture
- [ ] Référentiels (natures, acteurs, actions, contraintes) administrables
- [ ] Migration des écrans `MecanicLog` / `Analyse` vers `features/issues`
- [ ] Liaison d'une action à une intervention du carnet ; tableau de bord trié par blocage
- [x] Déploiement continu : images `dev` à chaque merge sur `master`, branche `dev` retirée
      (spec technique `docs/specs/technique-deploiement-continu.md`)

## Phase 5 — Coûts

- [ ] Dépenses rattachées au véhicule (entretien, réparation, assurance…), liées aux interventions et actions
- [ ] Synthèse par véhicule et par période, coût au kilomètre

## Phase 6 — Documents

- [ ] Pièces jointes du véhicule (carte grise, attestation d'assurance…)

## Phase 7 — Notifications

- [ ] Alertes d'échéances et de problèmes
- [ ] Notifications (abonnements push) reprises dans la nouvelle API

## Phase 8 — Alimentation de la base

- [ ] Import de l'historique (km, échéances) sur le modèle de l'import véhicules
- [ ] Import des problèmes, analyses et actions de l'ancienne API
- [ ] Port « source de kilométrage » pour une source externe (géoloc, télématique…)

## Phase 9 — Authentification et nettoyage

- [ ] Rôles Keycloak (lecture seule / gestionnaire)
- [ ] Réactivation et vérification de l'authentification sur tous les environnements
- [ ] Suppression du code front historique (`components/`, `services/`, `hooks/`)
- [ ] Retrait de `APP_LEGACY_API_URL` et arrêt de l'ancienne API
