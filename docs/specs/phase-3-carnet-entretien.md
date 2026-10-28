# Phase 3 — Carnet d'entretien

Statut : validée le 2026-10-28

## Contexte et objectif

La phase 2 suit les échéances et enregistre leurs réalisations (date, km, commentaire). La phase 3
ajoute le carnet d'entretien : l'historique des interventions réalisées sur chaque véhicule, prévues
(liées à une ou plusieurs échéances) ou imprévues (réparation sans échéance).

Principe retenu : **rester simple**. Le détail (coûts, pièces jointes, durée d'immobilisation…)
sera ajouté plus tard selon le besoin réel.

## Utilisateurs et rôles

- **Administrateur du parc** : seul profil (pas d'authentification avant la phase 9).

## User stories

### Must

**US1 — Gérer les garages**
En tant qu'administrateur, je veux une liste de garages, afin de saisir les interventions de façon homogène.
- [ ] Lister, créer, renommer, désactiver un garage ; nom unique (insensible à la casse) ; un garage utilisé ne peut pas être supprimé.
- [ ] Dans la saisie d'une intervention, le champ garage propose les garages actifs et permet d'en créer un nouveau en tapant son nom.

**US2 — Saisir une intervention**
En tant qu'administrateur, je veux saisir une intervention depuis la fiche véhicule, afin de tenir le carnet d'entretien.
- [ ] Champs : date (aujourd'hui par défaut), km, garage (facultatif), travaux (texte libre, obligatoire).
- [ ] Échéances réalisées : cases à cocher parmi les échéances en cours du véhicule (aucune = intervention imprévue).
- [ ] Chaque échéance cochée est réalisée selon les règles de la phase 2 (périodique : suivante recalculée depuis la date et le km réels ; ponctuelle : close).
- [ ] Le km crée un relevé kilométrique avec les mêmes contrôles (motif si incohérence, confirmation au-delà de 1 000 km/jour).
- [ ] Le km est obligatoire si une échéance cochée a un critère km.
- [ ] Date future refusée ; véhicule sorti du parc : saisie impossible.
- [ ] Tout est enregistré en une seule transaction.

**US3 — « Marquer réalisée » devient un raccourci**
- [ ] « Marquer réalisée » sur une échéance (fiche et tableau de bord) crée une intervention liée à cette seule échéance, avec garage facultatif.
- [ ] Les réalisations déjà enregistrées en phase 2 sont reprises comme interventions (migration de données).

**US4 — Consulter le carnet**
- [ ] Section « Carnet d'entretien » sur la fiche véhicule : interventions du plus récent au plus ancien (date, km, garage, travaux, échéances réalisées).

**US5 — Supprimer une intervention**
- [ ] Suppression après confirmation, avec un avertissement : les échéances qu'elle avait réalisées **ne sont pas recalculées** et le relevé kilométrique créé reste en place.

## Règles métier

- Une intervention appartient à un véhicule ; elle réalise 0, 1 ou plusieurs échéances **de ce véhicule**, chacune au plus une fois.
- Une échéance close ne peut pas être cochée.
- Pas de modification d'une intervention en phase 3 : on la supprime et on la ressaisit (à faire évoluer plus tard).
- La suppression ne recalcule rien (sujet à revoir plus tard).

## Hors périmètre

- Coûts : phase 5. Pièces jointes (factures) : phase 6.
- Durée d'immobilisation, intervenants, pièces : plus tard, selon le besoin.
- Vue globale des interventions de tout le parc.
- Modification d'une intervention ; recalcul des échéances à la suppression.

## Impact sur le modèle de données

- **Garage** (nouveau, référentiel) : nom, actif.
- **Intervention** (nouveau) : véhicule, date, km facultatif, garage facultatif, travaux, date de saisie.
- **Lien intervention ↔ échéance** : une intervention réalise plusieurs échéances.
- **Réalisation d'échéance** (phase 2) : remplacée par l'intervention ; données existantes migrées
  (une intervention par réalisation, travaux = commentaire ou nom du type d'échéance).

## Questions ouvertes

Aucune à ce stade.
