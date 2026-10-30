# Phase 5 — Coûts

Statut : en cours de cadrage (2026-10-30)

## Contexte et objectif

Suivre ce que coûte chaque véhicule et le parc. Principe retenu : **rester simple** (un montant, sans détail
pièces / main d'œuvre ni distinction HT / TTC) et affiner à l'usage.

Trois familles de dépenses :

- **Ponctuelles** : entretien, réparation (liées au carnet d'entretien) et autres dépenses ponctuelles.
- **Utilisation courante** : carburant, péages.
- **Emprunts** : financement des véhicules, visibles seulement par le service financier.

Les sources externes (cartes carburant Total, télépéage Ulys) alimenteront les dépenses d'utilisation courante
en **phase 8**. En phase 5, la saisie est manuelle. Chaque source externe sera un port (Application) avec un
adaptateur (Infrastructure).

## Utilisateurs et rôles

- **Gestionnaire du parc** : saisit et consulte les dépenses ponctuelles et d'utilisation courante.
- **Service financier** : seul à voir et gérer les emprunts. Les rôles arrivent en phase 9 (pas
  d'authentification en développement, ADR 0001).

## User stories

### Must

**US1 — Coût d'une intervention**
En tant que gestionnaire, je veux saisir le montant d'une intervention du carnet d'entretien, afin de connaître
le coût de l'entretien et des réparations.
- [ ] Montant facultatif sur l'intervention (saisie, modification).
- [ ] Une action de résolution d'un problème reliée à une intervention n'a pas de montant propre : le coût
      est celui de l'intervention.

**US2 — Dépenses d'un véhicule**
En tant que gestionnaire, je veux enregistrer une dépense sur un véhicule, afin de suivre ses coûts hors
carnet d'entretien.
- [ ] Champs : date, famille (ponctuelle ou utilisation courante), catégorie, montant, libellé facultatif.
- [ ] Catégories administrables par famille (au départ : carburant, péage ; autre dépense ponctuelle).
- [ ] Liste des dépenses sur la fiche véhicule ; modification et suppression.

**US3 — Synthèse des coûts**
En tant que gestionnaire, je veux une synthèse des coûts, afin de comparer les véhicules et suivre le budget.
- [ ] Par véhicule et pour le parc, par mois et par année, avec un total par famille et par catégorie.
- [ ] Coût au kilomètre sur la période : total des dépenses ÷ km parcourus d'après les relevés kilométriques.

### Should

**US4 — Emprunts** (visibles par le service financier seulement)
- [ ] À préciser.

## Règles métier

- Montant unique par dépense, sans détail ni distinction HT / TTC (à affiner plus tard).
- Pas de répartition des coûts récurrents : chaque dépense est enregistrée à sa date.

## Hors périmètre

- Connecteurs Total et Ulys : phase 8.
- Détail des factures (pièces, main d'œuvre), TVA.

## Impact sur le modèle de données

- **Intervention** : montant facultatif.
- **Dépense** (nouveau) : véhicule, date, famille, catégorie, montant, libellé, source (manuelle ; plus tard
  Total, Ulys).
- **Catégorie de dépense** (nouveau, référentiel) : famille, nom, active.
- **Emprunt** : à préciser.

## Questions ouvertes

- Accès aux API Total et Ulys (contrat existant, accès technique à vérifier).
- Pièce jointe « facture » : maintenant ou en phase 6 (Documents) ?
- Emprunts : contenu et calendrier vis-à-vis des rôles (phase 9).
