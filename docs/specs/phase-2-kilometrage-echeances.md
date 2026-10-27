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

- **Administrateur du parc** : saisit périodiquement les relevés kilométriques et gère les
  échéances. En phase 2, sans authentification, c'est le seul profil.

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

### Relevés kilométriques

- Saisie manuelle et périodique par l'administrateur.
- Un relevé inférieur au relevé précédent est accepté **avec un motif obligatoire**
  (ex. compteur remplacé, erreur de saisie précédente).
- L'auteur du relevé n'est pas enregistré en phase 2 : il viendra de l'authentification
  (phase 9) ou de la source externe (phase 8).
- Date du relevé : préremplie avec la date du jour, modifiable pour saisir un relevé passé
  (rattrapage). Une date dans le futur est refusée.
- Cohérence, évaluée par rapport aux relevés voisins **dans l'ordre des dates** (y compris
  pour un relevé passé inséré entre deux autres) :
  - valeur inférieure au relevé précédent ou supérieure au suivant : acceptée avec motif ;
  - plus de **1 000 km par jour** en moyenne depuis le relevé précédent : avertissement,
    enregistrement après confirmation.
- Le kilométrage actuel d'un véhicule est celui du relevé le plus récent par date.
- Par défaut (pas d'avis du propriétaire) : l'administrateur peut modifier ou supprimer un
  relevé, sans trace de l'ancienne valeur. Les mêmes contrôles s'appliquent à la modification.

### Échéances

- **Types d'échéance** : référentiel administrable (l'administrateur ajoute ses types). Types
  fournis au départ : contrôle technique, assurance, entretien constructeur, agrément ARS,
  matériel médical, oxygène, extincteur.
- **Périodicité** : en mois et/ou en km, **modifiable pour chaque véhicule**.
- Échéance à double critère (km et date) : elle tombe au **premier des deux atteint**.
- **Échéance ponctuelle** : simple date (et/ou km) sans périodicité, ex. fin d'un contrat
  d'assurance.
- **Échéance proche** : seuils globaux par défaut (30 jours ou 1 000 km avant), modifiables par
  l'administrateur, et **surchargeables par type** (ex. 60 jours pour l'agrément ARS).
- **Création d'un type** : l'application propose de l'ajouter aux véhicules d'un type de
  véhicule donné (voir questions ouvertes).
- **Échéance réalisée** : la suivante est calculée à partir de la date et du km **réels** de
  réalisation (CT prévu le 1er mars, fait le 15 mars : suivant le 15 mars de l'année suivante).
  Une échéance ponctuelle réalisée n'en génère pas de nouvelle.
- Statut du véhicule : les échéances d'un véhicule sorti du parc sont masquées, celles d'un
  véhicule immobilisé restent affichées (règle susceptible d'être revue).
- Statuts affichés : à jour, proche, dépassée.

## Hors périmètre

- Import de l'historique (km, échéances) et source de kilométrage externe : phase 8.
- Carnet d'entretien (interventions, garage, travaux) : phase 3.
- Notifications : phase 7.
- Rôles et droits : phase 9.

## Impact sur le modèle de données

À préciser.

## Questions ouvertes

- « Type de véhicule » : notion absente du modèle actuel (marque, modèle seulement), à définir.
- Traçabilité des corrections de relevés : à revoir avec l'authentification (phase 9).
