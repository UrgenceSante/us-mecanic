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
- [ ] Route d'API de signalement : véhicule, date, description, auteur du signalement. Le contrat sera
      affiné à l'usage.
- [ ] L'auteur du signalement est obligatoire : identifiant texte (nom ou matricule) envoyé par l'app
      chauffeur ; il viendra du jeton Keycloak en phase 9.
- [ ] Route authentifiée par défaut ; toutes les applications partagent le même Keycloak (rôles en phase 9).
- [ ] Le problème créé a le statut « Signalé ».

**US2 — Analyser un problème**
- [ ] L'administrateur renseigne nature, élément concerné, analyse (texte) et immobilisation (oui/non).
- [ ] Le problème passe au statut « Analysé ».
- [ ] Immobilisation cochée : le véhicule passe automatiquement au statut « immobilisé ».

**US3 — Actions de résolution**
- [ ] Une analyse porte 0..n actions : acteur, type, contrainte, échéance, commentaire, clôture.
- [ ] Une action peut être reliée à une intervention du carnet d'entretien du même véhicule.
- [ ] Quand toutes les actions sont closes, le problème passe automatiquement à « Résolu ».
- [ ] Le véhicule revient en service quand plus aucun problème immobilisant n'est ouvert.
- [ ] Un problème résolu peut être rouvert (par exemple en ajoutant une action) : il repasse à « Analysé »
      et, s'il est immobilisant, le véhicule repasse immobilisé.

**US4 — Référentiels administrables**
- [ ] Natures, éléments concernés, acteurs, types d'action, contraintes : créer, renommer, désactiver.
- [ ] Valeurs de départ : les listes actuelles de l'ancienne API.

**US5 — Écrans**
- [ ] Les vues « Problèmes » (tableau de bord et fiche véhicule) lisent la nouvelle API ; migration des
      écrans `MecanicLog` / `Analyse` vers `features/issues`.
- [ ] Tableau de bord : problèmes non résolus triés du plus au moins bloquant (immobilisants, puis
      signalés non analysés, puis actions en retard, puis les autres ; à égalité, le plus ancien d'abord).
- [ ] Filtres : statut, véhicule, nature.

### Should

**US6 — Reprise de l'historique**
- [ ] Mécanisme d'import des problèmes, analyses et actions de l'ancienne API, s'il reste simple.

## Règles métier

- Statuts : Signalé → Analysé → Résolu (résolution automatique). Susceptible d'évoluer.
- L'identité de l'auteur d'un signalement est une donnée personnelle : jamais dans les logs. Conservée
  sans limite pour la traçabilité ; une durée de conservation pourra être fixée en phase 9 (RGPD).
- Les champs « Rappel » et « CallBack » de l'ancienne action ne sont pas repris.

## Hors périmètre

- Application chauffeur (hors repo).

## Impact sur le modèle de données

- **Problème** : véhicule, date, description, auteur du signalement, statut.
- **Analyse** : nature, élément, texte, immobilisation.
- **Action** : acteur, type, contrainte, échéance, commentaire, close, intervention liée facultative.
- **Référentiels** : nature, élément concerné, acteur, type d'action, contrainte.

## Questions ouvertes

- Authentification et rôles de l'application chauffeur : phase 9.
