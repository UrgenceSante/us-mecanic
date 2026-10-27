# ADR-0001 : Pas d'authentification en développement

- Statut : accepté
- Date : 2026-10-27
- Décideur : propriétaire du projet

## Contexte

Tous les endpoints de l'API exigent un jeton Keycloak (fallback policy) et le front impose la
connexion. Pendant la construction des fonctionnalités (phases 2 à 8), cette contrainte ralentit
le développement et les essais. Les rôles Keycloak (lecture seule / gestionnaire) ne sont pas
encore définis et sont reportés à la phase 9. Les environnements de dev ne contiennent que des
données de développement.

## Options étudiées

1. Garder l'authentification partout : sûr, mais friction à chaque essai, en local comme sur le
   serveur de dev.
2. Désactiver l'authentification en développement seulement (local et environnement de dev
   déployé), la garder en prod : développement fluide, prod protégée.
3. Désactiver partout jusqu'à la phase 9 : le plus simple, mais la prod serait ouverte.

## Décision

Option 2. L'authentification est désactivée par configuration, en local et sur l'environnement
de dev déployé ; elle reste obligatoire en prod.

## Conséquences

- La désactivation est un réglage explicite de configuration, désactivé par défaut : une
  configuration absente ou inconnue doit laisser l'authentification active.
- Les données de l'environnement de dev sont considérées comme publiques : aucune donnée réelle
  (véhicules, conducteurs, positions GPS) ne doit y être chargée.
- L'API géoloc reste une API distincte, non concernée par ce réglage.
- Phase 9 : définir les rôles et réactiver l'authentification sur tous les environnements.
