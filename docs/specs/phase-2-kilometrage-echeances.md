# Phase 2 — Kilométrage et échéances

Statut : brouillon, en attente de validation

## Contexte et objectif

Le parc est enregistré (phase 1). La phase 2 permet de suivre le kilométrage de chaque véhicule
et ses échéances réglementaires et d'entretien, pour qu'aucune échéance ne passe inaperçue.

Décisions de cadrage (voir `docs/ROADMAP.md`) :

- Fonctionnalités d'abord : l'import de l'historique et la source de kilométrage externe
  (géoloc, télématique) sont reportés en phase 8. En phase 2, le kilométrage est saisi à la main.
- Pas d'authentification en développement (ADR 0001) ; rôles Keycloak reportés en phase 9.
- Le carnet d'entretien (interventions réalisées, garage, travaux) est en phase 3.

## Utilisateurs et rôles

- **Administrateur du parc** : seul profil en phase 2. Saisit les relevés kilométriques, gère
  les types de véhicule, les types d'échéance, les seuils et les échéances des véhicules.

## User stories

### Must

**US0 — Développer sans connexion (prérequis technique)**
En tant que développeur, je veux utiliser l'application sans me connecter en local et sur
l'environnement de dev, afin d'accélérer le développement.
- [ ] Un réglage de configuration désactive l'authentification de l'API et la connexion du front.
- [ ] Sans ce réglage (valeur absente ou inconnue), l'authentification reste active.
- [ ] La configuration de prod n'active pas ce réglage.

**US1 — Saisir un relevé kilométrique**
En tant qu'administrateur, je veux saisir le kilométrage d'un véhicule à une date, afin de
suivre son usage et ses échéances au km.
- [ ] Depuis la fiche véhicule : km (entier positif), date préremplie à aujourd'hui et modifiable.
- [ ] Date dans le futur : refusée.
- [ ] Valeur inférieure au relevé précédent (ou supérieure au suivant, par date) : motif
      obligatoire, sinon refus.
- [ ] Plus de 1 000 km/jour en moyenne depuis le relevé précédent : avertissement, enregistré
      seulement après confirmation.
- [ ] Impossible sur un véhicule sorti du parc.

**US2 — Consulter l'historique et le km actuel**
En tant qu'administrateur, je veux voir les relevés d'un véhicule et son km actuel, afin de
connaître son état.
- [ ] La fiche affiche le km actuel (relevé le plus récent par date) et sa date.
- [ ] La liste du parc affiche une colonne « km actuel ».
- [ ] L'historique liste les relevés du plus récent au plus ancien, avec le motif éventuel.

**US3 — Corriger ou supprimer un relevé**
En tant qu'administrateur, je veux corriger ou supprimer un relevé erroné, afin que le km
actuel et les échéances restent justes.
- [ ] Modification soumise aux mêmes contrôles que la saisie.
- [ ] Suppression après confirmation ; le km actuel et les échéances sont recalculés.

**US4 — Gérer les types de véhicule**
En tant qu'administrateur, je veux gérer les types d'utilisation des véhicules (ambulance, VSL,
sang…), afin de regrouper le parc et d'appliquer des échéances par type.
- [ ] Créer, renommer, désactiver un type ; un type utilisé ne peut pas être supprimé.
- [ ] Le formulaire véhicule propose le type (facultatif) ; la liste du parc filtre par type.

**US5 — Gérer les types d'échéance**
En tant qu'administrateur, je veux gérer les types d'échéance, afin de couvrir toutes les
obligations du parc.
- [ ] Types fournis au départ : contrôle technique, assurance, entretien constructeur,
      agrément ARS, matériel médical, oxygène, extincteur.
- [ ] Un type a : nom, périodicité par défaut (mois et/ou km, ou aucune = ponctuel), seuils
      « proche » optionnels (sinon seuils globaux).
- [ ] À la création d'un type, l'application propose de l'ajouter à tous les véhicules d'un ou
      plusieurs types de véhicule, avec la périodicité par défaut.
- [ ] Un type utilisé ne peut pas être supprimé, seulement désactivé.

**US6 — Ajouter une échéance à un véhicule**
En tant qu'administrateur, je veux ajouter une échéance à un véhicule, afin de suivre sa
prochaine date ou son prochain km.
- [ ] Choix du type ; périodicité préremplie depuis le type, modifiable pour ce véhicule.
- [ ] Échéance périodique : saisie de la dernière réalisation (date et/ou km) ; l'application
      calcule la prochaine.
- [ ] Échéance ponctuelle : saisie directe de la date et/ou du km d'échéance.
- [ ] Un même type ne peut être actif qu'une fois par véhicule.

**US7 — Marquer une échéance réalisée**
En tant qu'administrateur, je veux indiquer qu'une échéance a été réalisée, afin que la
suivante soit planifiée.
- [ ] Saisie : date, km (prérempli avec le km actuel), commentaire libre.
- [ ] Le km saisi crée un relevé kilométrique (mêmes contrôles qu'US1).
- [ ] Échéance périodique : la suivante est calculée depuis la date et le km réels de réalisation.
- [ ] Échéance ponctuelle : elle est close, aucune nouvelle n'est créée.

**US8 — Tableau de bord des échéances**
En tant qu'administrateur, je veux voir les échéances proches et dépassées, afin d'agir à temps.
- [ ] Page d'accueil de l'application.
- [ ] Liste triée par urgence (dépassées d'abord, puis les plus proches).
- [ ] Filtres : véhicule, type de véhicule, type d'échéance, statut (proche, dépassée).
- [ ] Chaque ligne mène à la fiche véhicule et permet de marquer l'échéance réalisée.

**US11 — Problèmes sur le tableau de bord et la fiche véhicule**
En tant qu'administrateur, je veux voir les problèmes de tout le parc sur le tableau de bord et
ceux d'un véhicule sur sa fiche, afin d'avoir une vue complète au même endroit.
- [ ] Le tableau de bord a une vue « Problèmes » : la liste actuelle des logs mécaniques
      (ancienne page d'accueil), pour tout le parc.
- [ ] La fiche véhicule a une vue « Problèmes » : les mêmes logs filtrés sur son immatriculation.
- [ ] Transitoire : les données viennent de l'ancienne API, comme aujourd'hui ; elles seront
      reprises par la nouvelle API en phase 4.

### Should

**US9 — Régler les seuils « proche »**
En tant qu'administrateur, je veux régler les seuils globaux (défaut 30 jours / 1 000 km),
afin d'adapter l'alerte à l'organisation.
- [ ] Seuils globaux modifiables ; un seuil défini sur un type d'échéance l'emporte.

**US10 — Voir les échéances sur la fiche véhicule**
- [ ] La fiche liste les échéances du véhicule avec statut, prochaine date / prochain km et
      dernière réalisation.

## Règles métier

### Relevés kilométriques

- Saisie manuelle et périodique par l'administrateur ; l'auteur n'est pas enregistré en
  phase 2 (il viendra de l'authentification ou de la source externe).
- Cohérence évaluée par rapport aux relevés voisins **dans l'ordre des dates** :
  - inférieur au précédent ou supérieur au suivant : accepté avec motif obligatoire ;
  - plus de 1 000 km/jour depuis le précédent : avertissement et confirmation.
- Km actuel = relevé le plus récent par date.
- Modification et suppression libres, sans trace de l'ancienne valeur (à revoir en phase 9).

### Échéances

- Périodicité en mois et/ou en km, modifiable par véhicule. Sans périodicité : échéance
  ponctuelle.
- Double critère : l'échéance tombe au **premier des deux atteint**.
- Prochaine échéance = dernière réalisation + périodicité (date réelle et km réel).
- Statut : **dépassée** (date passée ou km actuel ≥ km d'échéance), **proche** (dans le seuil),
  sinon **à jour**.
- Seuils « proche » : globaux par défaut (30 jours ou 1 000 km), surchargeables par type.
- Véhicule sorti du parc : ses échéances sont masquées du tableau de bord ; véhicule immobilisé :
  elles restent affichées (règle susceptible d'être revue).

## Hors périmètre

- Import de l'historique (km, échéances) et source de kilométrage externe : phase 8.
- Carnet d'entretien (interventions, garage, travaux, coûts) : phases 3 et 5.
- Notifications et alertes envoyées : phase 7.
- Rôles, droits, auteur des saisies : phase 9.
- Saisie directe de la prochaine date d'une échéance périodique (sans dernière réalisation).
- Colonne « type de véhicule » dans l'import CSV : phase 8 (en phase 2, le type se saisit dans
  le formulaire véhicule).
- Nouveau modèle de problèmes dans l'API : phase 4.

## Impact sur le modèle de données

- **Type de véhicule** (nouveau, référentiel) : nom, actif. Véhicule : type facultatif.
- **Relevé kilométrique** (nouveau) : véhicule, date, km, motif facultatif, date de saisie.
- **Type d'échéance** (nouveau, référentiel) : nom, périodicité par défaut (mois, km), seuils
  facultatifs (jours, km), actif.
- **Échéance d'un véhicule** (nouveau) : véhicule, type, périodicité (mois, km), date et km de
  dernière réalisation, date et km d'échéance, close ou non.
- **Réalisation** (nouveau) : échéance, date, km, commentaire. Base du futur carnet d'entretien.
- **Paramètres** (nouveau) : seuils globaux « proche ».

## Questions ouvertes

Aucune à ce stade.
