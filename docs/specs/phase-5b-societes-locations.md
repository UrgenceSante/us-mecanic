# Phase 5 bis — Sociétés et locations

Statut : décisions du propriétaire validées le 2026-11-17 ; choix complémentaires délégués (voir « Choix faits en
l'absence du propriétaire »), à confirmer.

## Contexte et objectif

Le groupe compte plusieurs sociétés : ambulances, pompes funèbres, transport de produits sanguins (PSL)… Chaque
véhicule appartient à l'une d'elles, et certaines louent des véhicules à une autre société du groupe. Le
gestionnaire du parc doit savoir, pour chaque véhicule, **à qui il appartient** et **qui l'utilisait à une date
donnée** : pour répartir les coûts, pour savoir qui appeler en cas de problème, pour qu'une société suive ses
propres véhicules.

Décisions du propriétaire (2026-11-17) :

1. **Sociétés du groupe seulement** : référentiel « Sociétés » administrable (créer, renommer, désactiver), sans
   loueur externe. Un véhicule a une société propriétaire.
2. **Périodes datées** : une location = société locataire + date de début + date de fin facultative. L'historique
   est conservé.
3. **Coûts** : filtres « Société propriétaire » et « Société locataire » sur la synthèse des coûts. Pas de
   refacturation ni de loyer calculé.
4. **Priorité** : avant la phase 6 (Documents).

Principe retenu : **rester simple**. Pas d'historique des propriétaires (un propriétaire courant), pas de contrat,
pas de montant de location.

## Utilisateurs et rôles

- **Gestionnaire du parc** : administre les sociétés, renseigne le propriétaire, enregistre les locations, consulte
  le parc, le tableau de bord et la synthèse par société.
- Pas d'authentification en développement (ADR 0001) ; la prod reste authentifiée. Les rôles arrivent en phase 9 :
  d'ici là, tout le monde voit toutes les sociétés. Une **visibilité par société** (une société ne voit que ses
  véhicules et ceux qu'elle loue) est envisagée pour la phase 9 ; elle n'est pas spécifiée ici (voir Questions
  ouvertes). Cette phase ne doit rien figer qui l'empêche : tout filtre par société passe par l'API.

## User stories

### Must

**US1 — Référentiel des sociétés**
En tant que gestionnaire, je veux tenir la liste des sociétés du groupe, afin de rattacher chaque véhicule et
chaque location à une société nommée de façon homogène.
- [ ] Lister, créer, renommer, désactiver une société, comme les autres référentiels (types de véhicule,
      catégories de dépense) ; pas de suppression.
- [ ] Nom obligatoire, nettoyé des espaces en début et fin, 100 caractères au plus, unique sans tenir compte de la
      casse (« Ambulances Nord » et « ambulances nord » : refus « Une société porte déjà ce nom »).
- [ ] Une société désactivée n'est plus proposée comme propriétaire ni comme locataire ; les véhicules et
      locations qui la citent la gardent et l'affichent avec la mention « (inactive) ». Elle reste proposée dans
      les filtres (parc, tableau de bord, coûts) pour consulter l'historique.
- [ ] Aucune valeur initiale : la liste est vide après la migration (les noms des sociétés du groupe ne sont pas
      connus de l'application).
- [ ] Écran « Sociétés » dans le menu « Administration ».

**US2 — Propriétaire d'un véhicule**
En tant que gestionnaire, je veux indiquer la société propriétaire d'un véhicule, afin de savoir à qui il
appartient.
- [ ] Champ « Société propriétaire » dans le formulaire du véhicule (création et modification), parmi les
      sociétés actives ; facultatif.
- [ ] Une fois renseigné, le propriétaire peut être changé mais pas vidé (refus « Le propriétaire ne peut pas être
      retiré, choisissez une autre société »).
- [ ] Given un véhicule avec une location en cours ou à venir dont le locataire est la société B, When je choisis
      B comme propriétaire, Then le changement est refusé avec le message « B loue ce véhicule : terminez ou
      supprimez d'abord cette location ».
- [ ] Fiche véhicule : le propriétaire est affiché en en-tête. Sans propriétaire, l'en-tête affiche « Propriétaire
      à renseigner » (mise en évidence d'avertissement, pas d'erreur) avec un accès direct à la modification.
- [ ] Changer de propriétaire ne réécrit rien : la synthèse par propriétaire utilise le propriétaire courant pour
      toutes les dépenses du véhicule (règle R7).

**US3 — Enregistrer une location**
En tant que gestionnaire, je veux enregistrer qu'une société du groupe loue un véhicule sur une période, afin de
savoir qui l'utilise.
- [ ] Champs : société locataire (obligatoire, parmi les sociétés actives), date de début (obligatoire), date de
      fin (facultative : location en cours sans terme connu).
- [ ] Refus si le véhicule n'a pas de propriétaire : « Renseignez d'abord le propriétaire du véhicule ».
- [ ] Refus si le locataire est le propriétaire : « Le locataire ne peut pas être le propriétaire ».
- [ ] Refus si la date de fin est antérieure à la date de début : « La date de fin doit être postérieure ou égale à
      la date de début ». Une location d'un seul jour (fin = début) est acceptée.
- [ ] Refus si la période chevauche une autre location du même véhicule, en nommant la location en conflit :
      « Chevauche la location à B du 01/02/2026 au 31/03/2026 » (ou « … depuis le 01/02/2026, sans date de fin »).
- [ ] Refus si le véhicule est sorti du parc : « Véhicule sorti du parc : aucune nouvelle location ».
- [ ] Une location peut commencer dans le futur (location prévue) ou dans le passé (reprise de l'historique).

**US4 — Terminer, corriger ou supprimer une location**
En tant que gestionnaire, je veux terminer une location en cours, corriger ses dates ou la supprimer si elle a été
saisie par erreur, afin que l'historique reste juste.
- [ ] « Terminer » sur une location sans date de fin : propose la date du jour, modifiable ; mêmes contrôles que
      l'US3 (fin ≥ début).
- [ ] Modification du locataire et des dates : mêmes contrôles que l'US3 (sauf « sorti du parc », voir R6).
- [ ] Suppression après une confirmation qui nomme la location (« Supprimer la location à B du 01/02/2026 au
      31/03/2026 ? Cette action est définitive. »).
- [ ] Sur un véhicule sorti du parc : terminer, modifier et supprimer restent possibles (clore la location à la
      date de sortie, corriger l'historique).

**US5 — Locations sur la fiche véhicule**
En tant que gestionnaire, je veux voir sur la fiche du véhicule son propriétaire, son utilisateur actuel et
l'historique de ses locations, afin de répondre en quelques secondes à « qui a ce véhicule ? ».
- [ ] Section « Propriétaire et locations » (placement validé par la passe `ux`) : propriétaire, utilisateur
      actuel (règle R4 appliquée à la date du jour), bouton « Ajouter une location ».
- [ ] Historique des locations, de la plus récente à la plus ancienne (date de début) : locataire, du, au (ou
      « en cours » / « prévue » selon la date du jour), actions Terminer / Modifier / Supprimer.
- [ ] Section sans location : « Aucune location : le véhicule est utilisé par son propriétaire » (ou « Propriétaire
      à renseigner » s'il n'y en a pas).
- [ ] En-tête de la fiche : « Loué à B depuis le JJ/MM/AAAA » quand une location couvre la date du jour.

**US6 — Propriétaire et utilisateur dans le parc**
En tant que gestionnaire, je veux voir et filtrer le parc par propriétaire et par utilisateur actuel, afin de
retrouver les véhicules d'une société ou ceux dont le propriétaire manque.
- [ ] Liste du parc (tableau) : colonnes « Propriétaire » et « Utilisateur actuel » (R4 à la date du jour) ;
      affichage en cartes sur téléphone : les deux informations figurent sur la carte.
- [ ] Filtre « Propriétaire » : Tous, chaque société, « À renseigner » (véhicules sans propriétaire).
- [ ] Filtre « Utilisateur actuel » : Tous, chaque société.
- [ ] Les filtres se combinent avec la recherche, le statut et le type existants (ET logique) et sont appliqués par
      l'API (paramètres de `GET /api/vehicles`), pas seulement côté navigateur.

**US7 — Synthèse des coûts par société**
En tant que gestionnaire, je veux filtrer la synthèse des coûts par société propriétaire et par société locataire,
afin de connaître ce que coûtent les véhicules d'une société et ceux qu'elle loue.
- [ ] Filtre « Société propriétaire » : toutes les dépenses (interventions, dépenses ponctuelles, utilisation
      courante, emprunts) des véhicules dont le propriétaire courant est la société choisie (R7).
- [ ] Filtre « Société locataire » : les dépenses dont la date tombe dans une location du véhicule à la société
      choisie (R8). Les mensualités d'emprunt ne sont jamais rattachées à un locataire (R8).
- [ ] Les deux filtres se combinent entre eux, avec le véhicule et avec la période (ET logique).
      Exemple : propriétaire A + locataire B = dépenses des véhicules de A pendant leurs locations à B.
- [ ] Avec le filtre « Société locataire », le coût au kilomètre affiche « Non calculé avec le filtre locataire »
      (R9).
- [ ] Un rappel sous les filtres explique la règle en une phrase : « Locataire : dépenses datées pendant une
      location ; les emprunts restent au propriétaire ».
- [ ] Pas de refacturation, de loyer ni de montant de location.

**US8 — Tableau de bord par société**
En tant que gestionnaire d'une société du groupe, je veux filtrer le tableau de bord sur ma société, afin de ne
voir que les échéances et problèmes qui me concernent.
- [ ] Filtre « Société » sur le tableau de bord : retient les véhicules dont la société est propriétaire **ou**
      utilisateur actuel (R4 à la date du jour), pour les échéances comme pour les problèmes.
- [ ] Se combine avec les filtres existants (véhicule, type de véhicule, type d'échéance, statut).

**US9 — Propriétaire dans l'import de véhicules**
En tant que gestionnaire, je veux indiquer le propriétaire dans le fichier d'import, afin de renseigner tout le
parc existant en une fois.
- [ ] Colonne facultative `Propriétaire` dans le CSV ; le modèle `docs/modele-import-vehicules.csv` et le README
      sont mis à jour.
- [ ] Rapprochement par nom de société, sans tenir compte de la casse ni des espaces en début et fin, parmi les
      sociétés actives.
- [ ] Nom inconnu ou société inactive : la ligne est en erreur dans le bilan (« Société inconnue ou inactive :
      X ») ; pas de création de société à la volée.
- [ ] Cellule vide : n'efface pas un propriétaire existant (règle actuelle de l'import) et laisse un nouveau
      véhicule sans propriétaire.
- [ ] Mêmes contrôles que l'US2 (propriétaire égal au locataire d'une location en cours ou à venir : ligne en
      erreur).
- [ ] La simulation (`dryRun`) montre le propriétaire qui sera appliqué.

### Should

- Aucune.

### Could

**US10 — Sociétés et locations dans le script de démonstration**
- [ ] `scripts/demo-seed.sh` crée 3 sociétés manifestement fictives (par exemple « Démo Ambulances », « Démo
      Pompes funèbres », « Démo Transport PSL »), attribue un propriétaire à chaque véhicule de démo sauf un
      (pour montrer « Propriétaire à renseigner »), et crée quelques locations : une terminée, une en cours, une
      prévue.
- [ ] Via l'API uniquement, rejouable selon les règles actuelles du script (réutilisation par nom).

### Won't (cette phase)

- Voir Hors périmètre.

## Règles métier

- **R1 — Société** : société du groupe uniquement ; nom unique (sans casse), 100 caractères au plus ; jamais
  supprimée, seulement désactivée.
- **R2 — Propriétaire** : au plus un propriétaire courant par véhicule, sans historique. Facultatif tant qu'il
  n'est pas renseigné ; ensuite modifiable mais jamais vidé.
- **R3 — Location** : véhicule, société locataire, date de début, date de fin facultative. Les dates sont des jours
  calendaires (sans heure), **bornes incluses** : une location du 01/02 au 31/03 couvre le 31/03 ; la suivante peut
  commencer le 01/04, pas le 31/03.
  - fin ≥ début ;
  - locataire ≠ propriétaire courant (contrôlé à la création et à la modification de la location, et au changement
    de propriétaire pour les locations en cours ou à venir) ;
  - pas de chevauchement entre les locations d'un même véhicule ; conséquence : au plus une location sans date de
    fin, et elle est la dernière ;
  - un véhicule sans propriétaire ne peut pas être loué.
- **R4 — Utilisateur à une date** : la société locataire de la location qui couvre cette date ; sinon le
  propriétaire courant ; sinon « Non renseigné ». « Utilisateur actuel » = utilisateur à la date du jour (fuseau
  Europe/Paris, comme les échéances).
- **R5 — État d'une location** par rapport à la date du jour : « prévue » (début futur), « en cours » (couvre le
  jour), « terminée » (fin passée).
- **R6 — Véhicule sorti du parc** : aucune nouvelle location ; les locations existantes restent consultables,
  modifiables (dont « Terminer ») et supprimables. La sortie du parc ne termine pas automatiquement la location en
  cours : la fiche affiche « Location en cours à terminer » si c'est le cas.
- **R7 — Coûts par propriétaire** : une dépense est rattachée à la société propriétaire **courante** du véhicule,
  quelle que soit sa date. Un véhicule sans propriétaire n'est rattaché à aucune société.
- **R8 — Coûts par locataire** : une intervention, une dépense ponctuelle ou d'utilisation courante est rattachée à
  la société locataire de la location qui couvre **sa date** ; aucune location ne couvre la date = pas de
  locataire. Les mensualités d'emprunt ne sont jamais rattachées à un locataire : le financement reste au
  propriétaire.
- **R9 — Coût au kilomètre** : inchangé sans filtre locataire (y compris avec le filtre propriétaire). Non calculé
  avec le filtre locataire (les relevés ne sont pas découpés par période de location dans cette phase).
- **R10 — Données personnelles** : les sociétés sont des personnes morales ; cette phase n'ajoute ni position GPS
  ni identité de conducteur. Aucune donnée nouvelle n'est à exclure des logs au-delà des règles existantes.

## Choix faits en l'absence du propriétaire

| Sujet | Choix | Raison |
|---|---|---|
| Véhicules existants sans propriétaire | Propriétaire facultatif (colonne nullable), **sans reprise automatique** ; « Propriétaire à renseigner » sur la fiche et filtre « À renseigner » dans le parc ; renseignement par le formulaire ou l'import (US9) | Rendre le champ obligatoire imposerait une valeur par défaut dans une migration, donc une modification de données soumise à l'accord du propriétaire, et une valeur fausse pour une partie du parc. L'import permet de tout renseigner en une fois. |
| Propriétaire une fois renseigné | Modifiable, jamais vidé | Un véhicule a toujours un propriétaire ; une erreur se corrige en choisissant la bonne société. Évite les locations dont on ne sait plus contrôler le locataire. |
| Historique des propriétaires | Non : propriétaire courant seulement | Décision 1 du propriétaire (« un véhicule a une société propriétaire ») ; les transferts entre sociétés du groupe sont rares. À rouvrir si la synthèse par propriétaire doit refléter un transfert (voir Questions ouvertes). |
| Location d'un véhicule sans propriétaire | Refusée | La règle « locataire ≠ propriétaire » n'est pas vérifiable sans propriétaire, et « utilisateur » perdrait son sens hors location. |
| Bornes des dates | Jours calendaires, bornes incluses | C'est ainsi qu'un gestionnaire lit « du 01/02 au 31/03 ». |
| Chevauchement | Interdit, y compris avec une location sans fin ; le message nomme la location en conflit | Un véhicule n'a qu'un utilisateur à la fois ; nommer le conflit évite de chercher. |
| Locations futures et passées | Autorisées | Planifier une location prévue ; reprendre l'historique connu. |
| Véhicule sorti du parc | Pas de nouvelle location ; terminer, modifier, supprimer restent possibles ; pas de fin automatique | Clore la location à la date réelle de sortie, que l'application ne connaît pas précisément. |
| Société désactivée | Plus proposée en saisie, conservée et affichée « (inactive) », toujours présente dans les filtres | Comme les autres référentiels ; consulter l'historique d'une société dissoute reste utile. |
| Valeurs initiales des sociétés | Aucune | Les noms des sociétés du groupe ne sont pas connus ; insérer des noms supposés serait faux. |
| Utilisateur à une date | Locataire si une location couvre la date, sinon propriétaire | Règle simple, explicable en une phrase, demandée par l'opérateur. |
| Coûts par propriétaire | Propriétaire courant, toutes dates | Conséquence directe de l'absence d'historique des propriétaires. |
| Coûts par locataire | Par la date de la dépense ; emprunts exclus | Simple et explicable ; un emprunt finance le véhicule pour le propriétaire, pas pour celui qui le loue quelques mois. |
| Coût au km avec filtre locataire | Non calculé, message explicite | Le découper par période de location demanderait des relevés aux bornes de chaque location : complexité non demandée. |
| Parc | Colonnes **et** filtres Propriétaire et Utilisateur actuel, filtre « À renseigner » | Les colonnes répondent à « qui ? », les filtres à « lesquels ? » ; « À renseigner » sert la reprise de l'existant. |
| Tableau de bord | Un filtre « Société » = propriétaire **ou** utilisateur actuel | Une société suit à la fois les échéances de ses véhicules (même loués) et les problèmes des véhicules qu'elle utilise ; un seul filtre reste simple. |
| Import | Colonne `Propriétaire` facultative, rapprochée par nom ; nom inconnu = ligne en erreur ; pas de création à la volée | Évite de créer des doublons par faute de frappe (« Ambulance Nord » / « Ambulances Nord »). |
| Montant de location, contrat | Hors périmètre | Décision 3 du propriétaire (pas de refacturation ni de loyer). |
| Script de démo | Could : 3 sociétés fictives, locations terminée / en cours / prévue | Montre la fonction sans être indispensable à la livraison. |

## Hors périmètre

- Loueurs ou propriétaires externes au groupe (loueurs longue durée, crédit-bail) : décision 1.
- Refacturation entre sociétés, loyer, montant ou contrat de location : décision 3. Le contrat de location se
  joindra au véhicule en phase 6 (catégorie « Contrat de location ou de crédit »).
- Historique des propriétaires (transferts entre sociétés) et synthèse selon le propriétaire à la date.
- Coût au kilomètre par locataire ; répartition des emprunts entre propriétaire et locataire.
- Filtre « Utilisateur » dans la synthèse des coûts (dépenses du propriétaire hors location + dépenses du
  locataire) : voir Questions ouvertes.
- Visibilité ou droits par société : phase 9.
- Informations de la société (SIRET, adresse, contact, logo).
- Alertes de fin de location : phase 7 (notifications), si le besoin est confirmé.
- Rattachement des problèmes, interventions ou dépenses à une société saisie à la main : ils se déduisent de la
  date (R4, R8).

## Impact sur le modèle de données

- **Société** (nouveau, référentiel) : identifiant, nom (unique sans casse, 100 caractères), active.
  Aucune valeur initiale.
- **Véhicule** : nouvelle colonne **propriétaire** (référence à une société), **nullable**, sans valeur par
  défaut ni reprise : tous les véhicules existants restent sans propriétaire après la migration.
- **Location** (nouveau) : identifiant, véhicule, société locataire, date de début, date de fin (nullable).
  Index par véhicule et dates. L'absence de chevauchement est garantie par l'API (règle du Domain) ; une
  contrainte d'exclusion PostgreSQL peut la doubler, créée avec la table.
- **Dépense, Intervention, Emprunt, Relevé** : inchangés ; le rattachement à une société se calcule (R7, R8).
- **API** : nouveaux paramètres de filtre sur `GET /api/vehicles` (propriétaire, « à renseigner », utilisateur
  actuel), sur la synthèse des coûts (propriétaire, locataire) et sur le tableau de bord (société). La réponse
  véhicule expose propriétaire et utilisateur actuel.

### Migrations et accord du propriétaire

| Migration | Nature | Auto-merge |
|---|---|---|
| Création de la table Société | Création de table, vide | Éligible (création de table) |
| Ajout de la colonne propriétaire sur Véhicule | Ajout d'une colonne nullable et de sa clé étrangère, aucune ligne modifiée | Éligible (aucune donnée modifiée) ; `reviewer` vérifie qu'il n'y a ni valeur par défaut ni `UPDATE` |
| Création de la table Location (index, contrainte d'exclusion éventuelle) | Création de table, vide | Éligible (création de table) |

**Aucune migration de cette phase ne modifie ni ne supprime de données existantes.** Si le propriétaire souhaite
une reprise automatique des propriétaires (par exemple « tous les véhicules existants à la société X »), ce sera
une migration de données distincte, soumise à son accord explicite, sans auto-merge. L'alternative recommandée est
l'import (US9), qui passe par la simulation et le bilan ligne par ligne.

## Décisions complémentaires (orchestrateur, 2026-11-17)

- **Véhicule sorti du parc sans propriétaire** : la saisie du propriétaire reste possible si le champ est vide,
  et seulement ce champ (le reste du formulaire reste verrouillé). Ainsi, les coûts passés se rattachent à une société.
- **Emprunts** : les mensualités restent toujours au propriétaire (R8 confirmé).

## Questions ouvertes

- **Noms des sociétés** : liste exacte des sociétés du groupe (à saisir par l'opérateur dans l'écran
  « Sociétés » ; aucune valeur initiale n'est prévue).
- **Transferts entre sociétés** : un véhicule peut-il changer de société propriétaire dans le groupe ? Si oui et
  si la synthèse doit refléter le propriétaire à la date, il faudra un historique des propriétaires (phase
  ultérieure).
- **Filtre « Utilisateur » dans la synthèse** : une société voudra peut-être « tout ce que j'ai utilisé » (ses
  véhicules hors location + ceux qu'elle loue), selon R4. À confirmer à l'usage ; non demandé pour l'instant.
- **Phase 9 — visibilité par société** : une société ne verrait que les véhicules dont elle est propriétaire ou
  utilisatrice ? Qui voit l'historique des locations d'un véhicule loué à une autre société ? Le rattachement d'un
  utilisateur Keycloak à une ou plusieurs sociétés est à concevoir en phase 9.
