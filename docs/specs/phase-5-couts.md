# Phase 5 — Coûts

Statut : en cours de cadrage (2026-10-30)

## Contexte et objectif

Suivre ce que coûte chaque véhicule et le parc. Principe retenu : **rester simple** (un montant, sans détail
pièces / main d'œuvre ni distinction HT / TTC) et affiner à l'usage.

Trois familles de dépenses :

- **Ponctuelles** : entretien, réparation (liées au carnet d'entretien) et autres dépenses ponctuelles.
- **Utilisation courante** : carburant, péages.
- **Emprunts** : financement des véhicules, visibles seulement par le service financier.

Les dépenses d'utilisation courante (carburant, péages) ne sont **jamais saisies à la main** : elles viendront
des sources externes (cartes Total, télépéage Ulys) en **phase 8**, ou pas du tout. Chaque source externe sera
un port (Application) avec un adaptateur (Infrastructure). La phase 5 prévoit leur place dans le modèle et la
synthèse.

## Utilisateurs et rôles

- **Gestionnaire du parc** : saisit et consulte les dépenses ponctuelles et d'utilisation courante.
- **Service financier** : seul à voir et gérer les emprunts. Les rôles arrivent en phase 9 (pas
  d'authentification en développement, ADR 0001) : d'ici là, les emprunts sont visibles de tous, avec des
  données fictives uniquement.

## User stories

### Must

**US1 — Coût d'une intervention**
En tant que gestionnaire, je veux saisir le montant d'une intervention du carnet d'entretien, afin de connaître
le coût de l'entretien et des réparations.
- [ ] Montant facultatif sur l'intervention (saisie, modification).
- [ ] Une action de résolution d'un problème reliée à une intervention n'a pas de montant propre : le coût
      est celui de l'intervention.

**US2 — Dépenses ponctuelles d'un véhicule**
En tant que gestionnaire, je veux enregistrer une dépense ponctuelle hors carnet d'entretien, afin de suivre
tous les coûts d'un véhicule.
- [ ] Champs : date, catégorie, montant, libellé facultatif.
- [ ] Catégories administrables (création, renommage, désactivation).
- [ ] Liste des dépenses sur la fiche véhicule ; modification et suppression.
- [ ] Les dépenses d'utilisation courante (carburant, péage) ne sont pas saisissables : elles n'arriveront que
      par les connecteurs de la phase 8.

**US3 — Synthèse des coûts**
En tant que gestionnaire, je veux une synthèse des coûts, afin de comparer les véhicules et suivre le budget.
- [ ] Par véhicule et pour le parc, par mois et par année, avec un total par famille et par catégorie.
- [ ] Coût au kilomètre sur la période : total des dépenses ÷ km parcourus d'après les relevés kilométriques.

**US4 — Emprunts**
En tant que service financier, je veux enregistrer l'emprunt qui finance un véhicule, afin d'intégrer son coût
à la synthèse.
- [ ] Champs : véhicule, organisme, montant emprunté, mensualité, date de début, durée (mois).
- [ ] Les mensualités sont comptées dans la synthèse, à leur mois d'échéance, dans la famille « Emprunts ».
- [ ] Modification et suppression.
- [ ] Visibilité réservée au service financier à partir de la phase 9 (rôles) ; données fictives d'ici là.

## Règles métier

- Montant unique par dépense, sans détail ni distinction HT / TTC (à affiner plus tard).
- Pas de répartition des coûts récurrents : chaque dépense est enregistrée à sa date.

## Hors périmètre

- Connecteurs Total et Ulys (carburant, péages) : phase 8.
- Facture en pièce jointe : phase 6 (Documents).
- Détail des factures (pièces, main d'œuvre), TVA.

## Impact sur le modèle de données

- **Intervention** : montant facultatif.
- **Dépense** (nouveau) : véhicule, date, famille (ponctuelle, utilisation courante), catégorie, montant,
  libellé, source (manuelle ; plus tard Total, Ulys).
- **Catégorie de dépense** (nouveau, référentiel) : nom, active.
- **Emprunt** (nouveau) : véhicule, organisme, montant emprunté, mensualité, date de début, durée.

## Questions ouvertes

- Accès technique aux API Total et Ulys (contrat existant) : à vérifier avant la phase 8.
