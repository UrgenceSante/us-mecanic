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

- [x] Signalements de problème rattachés au véhicule
- [x] Analyse : nature, élément concerné, immobilisation (lien avec le statut du véhicule)
- [x] Actions de résolution : acteur, type, contrainte, échéance, clôture
- [x] Référentiels (natures, acteurs, actions, contraintes) administrables
- [x] Migration des écrans `MecanicLog` / `Analyse` vers `features/issues`
- [x] Liaison d'une action à une intervention du carnet ; tableau de bord trié par blocage
- [x] Déploiement continu : images `dev` à chaque merge sur `master`, branche `dev` retirée
      (spec technique `docs/specs/technique-deploiement-continu.md`)

## Phase 5 — Coûts

Spec : `docs/specs/phase-5-couts.md`

- [x] Montant d'une intervention ; dépenses ponctuelles et catégories administrables
- [x] Emprunts des véhicules (mensualités)
- [x] Synthèse par véhicule et par période, coût au kilomètre

## Jalon — v1 démontrable

Spec : `docs/specs/v1-demontrable.md`

- [x] Script de données de démonstration (via l'API)
- [x] Passe UX globale et corrections prioritaires
- [x] Parcours de démonstration documenté

## Phase 5 bis — Sociétés et locations

Spec : `docs/specs/phase-5b-societes-locations.md`.
Sociétés du groupe (ambulances, pompes funèbres, transport PSL…), société propriétaire de chaque véhicule,
locations datées entre sociétés. Aucune migration ne modifie de données existantes.

- [x] Référentiel « Sociétés » : API (créer, renommer, désactiver), écran « Administration »
- [x] Propriétaire du véhicule (API) : colonne nullable sans reprise, contrôles (non vidé, pas un locataire en cours),
      réponse véhicule avec propriétaire et utilisateur actuel
- [x] Propriétaire dans le formulaire et la fiche véhicule, mention « Propriétaire à renseigner »
- [x] Locations (API) : création, fin, modification, suppression ; règles (chevauchement, locataire ≠ propriétaire,
      fin ≥ début, sorti du parc) ; utilisateur à une date
- [x] Section « Propriétaire et locations » de la fiche véhicule : utilisateur actuel, historique, actions
- [x] Parc : colonnes et filtres Propriétaire (dont « À renseigner ») et Utilisateur actuel (API et front)
- [x] Import de véhicules : colonne `Propriétaire` facultative, modèle CSV et README
- [x] Synthèse des coûts : filtres Société propriétaire et Société locataire (API et front)
- [ ] Tableau de bord : filtre Société (propriétaire ou utilisateur actuel)
- [ ] Passe `ux` de la phase (ordinateur, iPad, téléphone) et corrections prioritaires
- [x] (Could) Sociétés fictives et locations dans le script de démonstration

## Phase 6 — Documents

Spec : `docs/specs/phase-6-documents.md` (stockage : `docs/decisions/0002-stockage-des-documents.md`).
Pièces jointes du véhicule (carte grise, attestation d'assurance…) et factures des interventions et dépenses.

- [ ] Catégories de document administrables : API, valeurs initiales, écran « Administration »
- [ ] Stockage des documents : port `IDocumentStorage`, adaptateur PostgreSQL (métadonnées et contenu séparés),
      contrôles du fichier (PDF / JPEG / PNG par signature, 10 Mo)
- [ ] Documents du véhicule (API) : ajout, liste avec les factures, ouverture / téléchargement, suppression
- [ ] Section « Documents » de la fiche véhicule : ajout (fichier ou photo), filtre, ouverture, téléchargement,
      suppression
- [ ] Factures des interventions et des dépenses (API) : ajout, liste, suppression en cascade
- [ ] Factures dans le carnet d'entretien et la liste des dépenses (front)
- [ ] (Should) Justificatif joint en marquant une échéance réalisée (fiche et tableau de bord)
- [ ] Passe `ux` de la phase (ordinateur, iPad, téléphone) et corrections prioritaires
- [ ] (Could) Documents fictifs dans le script de démonstration

## Phase 7 — Notifications

- [ ] Alertes d'échéances et de problèmes
- [ ] Notifications (abonnements push) reprises dans la nouvelle API

## Phase 8 — Alimentation de la base

- [ ] Import de l'historique (km, échéances) sur le modèle de l'import véhicules
- [ ] Import des problèmes, analyses et actions de l'ancienne API
- [ ] Port « source de kilométrage » pour une source externe (géoloc, télématique…)
- [ ] Dépenses d'utilisation courante : connecteurs cartes carburant Total et télépéage Ulys

## Phase 9 — Authentification et nettoyage

- [ ] Rôles Keycloak (lecture seule / gestionnaire / service financier pour les emprunts) ; visibilité par
      société envisagée (voir `docs/specs/phase-5b-societes-locations.md`), à spécifier
- [ ] Réactivation et vérification de l'authentification sur tous les environnements
- [ ] Suppression du code front historique (`components/`, `services/`, `hooks/`)
- [ ] Retrait de `APP_LEGACY_API_URL` et arrêt de l'ancienne API
