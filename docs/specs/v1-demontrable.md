# Jalon — v1 démontrable

Statut : cadré le 2026-11-17 par l'orchestrateur, à la demande du propriétaire (« une v1 démontrable qu'on fera
évoluer au besoin »), sans nouvelle fonctionnalité métier.

## Objectif

Pouvoir présenter l'application de bout en bout (parc, kilométrage et échéances, carnet d'entretien, problèmes,
coûts) sur l'environnement de dev, avec des données crédibles, sur ordinateur, iPad et téléphone.

## Périmètre

1. **Données de démonstration** : un script rejouable qui remplit une base vide via l'API publique (aucune écriture
   directe en base, aucune migration) : types de véhicule, une dizaine de véhicules aux statuts variés, relevés
   kilométriques sur 12 mois, échéances proches / dépassées / à jour, interventions avec montants, problèmes à
   tous les statuts (dont immobilisant), dépenses (dont prévues), emprunts. Données fictives uniquement.
2. **Passe UX globale** : l'agent `ux` parcourt tous les écrans avec ces données et liste les défauts par
   priorité ; les bloquants et les corrections rapides sont traités.
3. **Parcours de démonstration** documenté (README) : comment remplir la base et quoi montrer.

## Hors périmètre

- Phases 6 à 9 (documents, notifications, alimentation, authentification) : évolutions ultérieures.
- Changement de déploiement (Dockerfile, `deploy/`, CI) : décision du propriétaire.

## Règles

- Le script refuse de s'exécuter si l'API ne répond pas en mode « authentification désactivée » (garde-fou
  contre une exécution en prod).
- Aucune donnée personnelle réelle (immatriculations, noms) : valeurs manifestement fictives.
