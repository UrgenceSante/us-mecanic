# Phase 4 — Problèmes et résolution

Statut : en cours de cadrage (2026-10-29)

## Contexte et objectif

Les problèmes mécaniques sont aujourd'hui gérés par l'ancienne API (écrans `MecanicLog` et `Analyse`,
affichés en lecture via la vue « Problèmes » depuis la phase 2). La phase 4 reprend ce suivi dans la
nouvelle API : signalement, analyse, actions de résolution, référentiels administrables.

Principe retenu : reprendre le fonctionnement actuel simplement ; il évoluera à l'usage.

## Utilisateurs et rôles

- **Chauffeur** : signale un problème depuis son application (hors de ce repo), via une route de l'API.
- **Administrateur du parc** : analyse, définit et clôt les actions, administre les référentiels.

## User stories

### Must

**US1 — Signaler un problème (API)**
En tant que chauffeur, je veux signaler un problème sur un véhicule depuis mon application, afin qu'il
soit pris en charge.
- [ ] Route d'API de signalement : véhicule, date, description. Le contrat sera affiné à l'usage.
- [ ] Le problème créé a le statut « Signalé ».

**US2 — Analyser un problème**
- [ ] L'administrateur renseigne nature, élément concerné, analyse (texte) et immobilisation (oui/non).
- [ ] Le problème passe au statut « Analysé ».
- [ ] Immobilisation cochée : le véhicule passe automatiquement au statut « immobilisé ».

**US3 — Actions de résolution**
- [ ] Une analyse porte 0..n actions : acteur, type, contrainte, échéance, commentaire, clôture.
- [ ] Quand toutes les actions sont closes, le problème passe automatiquement à « Résolu » ; si le
      véhicule avait été immobilisé par ce problème, il revient en service.

**US4 — Référentiels administrables**
- [ ] Natures, éléments concernés, acteurs, types d'action, contraintes : créer, renommer, désactiver.

**US5 — Écrans**
- [ ] Les vues « Problèmes » (tableau de bord et fiche véhicule) lisent la nouvelle API ; migration des
      écrans `MecanicLog` / `Analyse` vers `features/issues`.

### Should

**US6 — Reprise de l'historique**
- [ ] Mécanisme d'import des problèmes, analyses et actions de l'ancienne API, s'il reste simple.

## Règles métier

- Statuts : Signalé → Analysé → Résolu (résolution automatique). Susceptible d'évoluer.

## Hors périmètre

- Application chauffeur (hors repo).

## Impact sur le modèle de données

- **Problème** : véhicule, date, description, statut.
- **Analyse** : nature, élément, texte, immobilisation.
- **Action** : acteur, type, contrainte, échéance, commentaire, close.
- **Référentiels** : nature, élément concerné, acteur, type d'action, contrainte.

## Questions ouvertes

- Identité de l'équipage sur le signalement (donnée personnelle).
- Authentification de l'application chauffeur.
