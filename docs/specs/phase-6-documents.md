# Phase 6 — Documents

Statut : validée le 2026-11-17 (choix délégués)

## Contexte et objectif

Le gestionnaire d'un parc d'ambulances, de véhicules funéraires et de transport de produits sanguins (PSL) doit
retrouver vite les papiers d'un véhicule : pour un contrôle routier ou une inspection ARS, pour un sinistre, pour
contester une facture. Aujourd'hui ces papiers sont dans des classeurs ou des boîtes mail.

La phase 6 permet de joindre des fichiers (PDF ou photo) :

- **au véhicule** : carte grise, attestation d'assurance, PV de contrôle technique, agrément ARS, attestation de
  désinfection, photos, contrat de location ou de crédit… ;
- **à une intervention du carnet ou à une dépense** : facture ou justificatif.

Principe retenu : **rester simple**. Un document = un fichier, une catégorie (pour le véhicule), un libellé
facultatif. Pas de modification, pas de version, pas de date d'expiration : les échéances (phase 2) suivent déjà
les dates de validité.

Stockage : PostgreSQL derrière un port `IDocumentStorage`, voir `docs/decisions/0002-stockage-des-documents.md`.

## Utilisateurs et rôles

- **Gestionnaire du parc** : ajoute, consulte, télécharge et supprime les documents.
- Pas d'authentification en développement (ADR 0001) ; la prod reste authentifiée. Les rôles (lecture seule,
  gestionnaire, service financier) arrivent en phase 9 : voir Questions ouvertes.

## User stories

### Must

**US1 — Catégories de document administrables**
En tant que gestionnaire, je veux une liste de catégories de document que je peux compléter, afin de classer les
papiers de chaque véhicule de façon homogène, y compris ceux propres à l'ambulance, au funéraire ou au PSL.
- [ ] Lister, créer, renommer, désactiver une catégorie (comme les autres référentiels) ; nom obligatoire,
      100 caractères au plus, unique sans tenir compte de la casse ; pas de suppression.
- [ ] Une catégorie désactivée n'est plus proposée à l'ajout ; les documents déjà classés la gardent et
      l'affichent.
- [ ] Valeurs initiales créées par la migration : Carte grise, Attestation d'assurance, Procès-verbal de contrôle
      technique, Agrément ARS, Attestation de désinfection, Attestation de conformité funéraire, Qualification
      thermique (PSL), Photo, Contrat de location ou de crédit, Autre.
- [ ] Écran dans le menu « Administration » (« Catégories de document »).

**US2 — Ajouter un document au véhicule**
En tant que gestionnaire, je veux joindre un fichier à un véhicule depuis sa fiche, sur ordinateur comme sur
téléphone, afin d'avoir ses papiers sous la main.
- [ ] Champs : fichier (obligatoire), catégorie (obligatoire, parmi les catégories actives), libellé facultatif
      (200 caractères au plus).
- [ ] Sur téléphone et tablette, le choix du fichier propose aussi l'appareil photo (prise de vue directe).
- [ ] Formats acceptés : PDF, JPEG, PNG. Le format est contrôlé par l'API sur le contenu du fichier, pas
      seulement sur son extension ; un autre format (HEIC, Word, ZIP…) est refusé avec le message « Format non
      accepté : PDF, JPEG ou PNG uniquement ».
- [ ] Taille maximale : 10 Mo par fichier. Au-delà : refus avec le message « Fichier trop volumineux (10 Mo
      maximum) », contrôlé avant l'envoi par le front et à la réception par l'API. Fichier vide refusé.
- [ ] Un fichier par ajout.
- [ ] Véhicule sorti du parc : ajout possible (certificat de cession, facture reçue après la sortie), consultation
      et suppression possibles.
- [ ] Pendant l'envoi, un indicateur est affiché et le bouton d'envoi est désactivé ; en cas d'échec, le
      message d'erreur est affiché et le formulaire garde la saisie.

**US3 — Consulter les documents du véhicule**
En tant que gestionnaire, je veux voir tous les documents d'un véhicule au même endroit, afin de retrouver un
papier en quelques secondes.
- [ ] Section « Documents » sur la fiche véhicule (placement validé par la passe `ux`).
- [ ] Tous les documents du véhicule y figurent, y compris les factures des interventions et des dépenses, du
      plus récent au plus ancien (date d'ajout).
- [ ] Pour chaque document : libellé (à défaut, nom du fichier d'origine), rattachement (catégorie pour un
      document du véhicule ; « Facture — intervention du JJ/MM/AAAA » ou « Facture — dépense du JJ/MM/AAAA »),
      date d'ajout, taille.
- [ ] Filtre par catégorie, avec une entrée « Factures » pour les documents d'intervention et de dépense.
- [ ] Section vide : message « Aucun document » et le bouton d'ajout.

**US4 — Ouvrir ou télécharger un document**
En tant que gestionnaire, je veux ouvrir un document dans le navigateur ou le télécharger, afin de le lire, de
l'imprimer ou de le transmettre.
- [ ] « Ouvrir » affiche le document dans un nouvel onglet (PDF et images lus par le navigateur).
- [ ] « Télécharger » enregistre le fichier sous son nom d'origine.
- [ ] Le fichier téléchargé est identique octet pour octet au fichier envoyé.
- [ ] Les deux fonctionnent en prod avec l'authentification : aucune URL publique, aucun jeton dans l'URL.
- [ ] Vérifié sur ordinateur, iPad et téléphone (passe `ux`).

**US5 — Supprimer un document**
En tant que gestionnaire, je veux supprimer un document joint par erreur ou périmé, afin de garder une liste
propre.
- [ ] Suppression après une confirmation qui nomme le document (« Supprimer « Carte grise 2026.pdf » ? Cette
      action est définitive. »).
- [ ] La suppression efface le contenu (pas de corbeille).
- [ ] Possible aussi sur un véhicule sorti du parc (retrait d'un document qui n'a pas à être conservé).

**US6 — Factures des interventions et des dépenses**
En tant que gestionnaire, je veux joindre la facture à une intervention du carnet ou à une dépense, afin de
justifier le montant et de pouvoir le contester.
- [ ] Ajout depuis la ligne de l'intervention (carnet d'entretien) ou de la dépense, sur une intervention ou une
      dépense déjà enregistrée : mêmes formats, taille et messages que l'US2, sans catégorie, libellé facultatif.
- [ ] Plusieurs documents possibles par intervention ou par dépense (facture, devis, bon de commande…).
- [ ] La ligne affiche un indicateur avec le nombre de documents ; depuis la ligne, on voit la liste et on peut
      ouvrir, télécharger, supprimer (règles des US4 et US5).
- [ ] Supprimer une intervention ou une dépense supprime ses documents : la confirmation existante le précise
      (« et ses N documents ») quand il y en a.
- [ ] Ces documents apparaissent aussi dans la section « Documents » du véhicule (US3).
- [ ] Les dépenses d'utilisation courante (connecteurs, phase 8) n'ont pas de pièce jointe.

**US7 — Données personnelles**
En tant que responsable du parc, je veux que les documents ne fuient pas, afin de protéger les données
personnelles qu'ils peuvent contenir (titulaire d'une carte grise, contrat, photo).
- [ ] Ni le contenu ni le nom des fichiers n'apparaissent dans les logs (seulement identifiant, type, taille).
- [ ] Tous les endpoints de documents sont authentifiés par la politique par défaut (aucun `AllowAnonymous`).
- [ ] Les fichiers sont servis avec leur type réel et sans exécution possible par le navigateur (seuls PDF, JPEG,
      PNG existent en base).
- [ ] Environnement de dev : documents fictifs uniquement (ADR 0001), rappelé dans le README.

### Should

**US8 — Joindre le justificatif en marquant une échéance réalisée**
En tant que gestionnaire, je veux joindre le PV de contrôle technique ou l'attestation au moment où je marque
l'échéance réalisée, afin de ne pas avoir à rouvrir le carnet ensuite.
- [ ] Le dialogue « Marquer réalisée » (fiche véhicule et tableau de bord) propose un fichier facultatif
      (mêmes formats, taille et prise de photo que l'US2).
- [ ] Le fichier est joint à l'intervention créée par la réalisation : il apparaît dans le carnet et dans la
      section « Documents » (comme une facture d'intervention).
- [ ] Given une réalisation valide et un fichier, When l'envoi du fichier échoue, Then la réalisation reste
      enregistrée et le message indique « Échéance réalisée ; le document n'a pas pu être joint, ajoutez-le
      depuis le carnet d'entretien ».
- [ ] Le fichier est contrôlé (format, taille) avant l'enregistrement de la réalisation, pour éviter ce cas
      dans les erreurs courantes.

### Could

- Documents fictifs (un PDF et une image générés) dans `scripts/demo-seed.sh`.
- Sélection de plusieurs fichiers en un seul ajout.
- Modification de la catégorie ou du libellé d'un document (aujourd'hui : supprimer et rajouter).

### Won't (cette phase)

- Voir Hors périmètre.

## Règles métier

- Un document appartient toujours à un véhicule. Il est rattaché soit au véhicule lui-même (catégorie
  obligatoire), soit à une intervention de ce véhicule, soit à une dépense ponctuelle de ce véhicule (sans
  catégorie). L'intervention ou la dépense doit appartenir au même véhicule.
- Formats : PDF, JPEG, PNG, reconnus par leur signature de contenu. Taille : de 1 octet à 10 Mo (10 × 1024 × 1024
  octets).
- Nom du fichier d'origine conservé pour l'affichage et le téléchargement, nettoyé (sans chemin ni caractères de
  contrôle, 255 caractères au plus).
- Un document ne se modifie pas : on le supprime et on en ajoute un autre.
- Suppression définitive, contenu compris. Supprimer une intervention ou une dépense supprime ses documents dans
  la même transaction.
- Véhicule sorti du parc : ajout, consultation et suppression possibles (documents de cession, factures tardives) ; seules les nouvelles interventions et dépenses restent refusées, comme avant.
- Pas de limite de nombre de documents par véhicule, intervention ou dépense.
- Les listes ne chargent jamais le contenu des fichiers.

## Choix faits en l'absence du propriétaire

| Sujet | Choix | Raison |
|---|---|---|
| Catégories des documents du véhicule | Référentiel administrable avec valeurs initiales | Aligné sur les autres référentiels (types de véhicule, d'échéance, garages, catégories de dépense) ; les besoins funéraire (attestation de conformité) et PSL (qualification thermique) s'ajoutent sans développement. |
| Valeurs initiales | Liste de l'US1, dont « Autre » | Couvre ambulance, funéraire et PSL dès le premier jour ; données de référentiel, pas des données personnelles. |
| Factures | Sans catégorie, plusieurs par intervention ou dépense | Le rattachement suffit à dire ce que c'est ; un devis et une facture vont souvent ensemble. |
| Vue d'ensemble | Toutes les pièces du véhicule dans la section « Documents », factures comprises | Un seul endroit pour chercher ; le PV de CT joint à une intervention reste trouvable côté véhicule. |
| Justificatif à la réalisation d'une échéance | Should, joint à l'intervention créée | Utile (PV de CT, attestation ARS) mais pas indispensable : l'US6 permet déjà de l'ajouter depuis le carnet. Découpé à part pour pouvoir être reporté. |
| Formats | PDF, JPEG, PNG ; HEIC refusé | Lus par tous les navigateurs. Un iPhone convertit ses photos en JPEG quand le champ n'accepte que ces formats, donc HEIC n'est pas nécessaire. |
| Taille | 10 Mo par fichier | Une photo de téléphone fait 2 à 5 Mo, un PDF scanné 1 à 3 Mo ; reste sous la limite par défaut du serveur web de l'API (30 Mo). |
| Prise de photo | Appareil photo proposé par le sélecteur de fichier sur mobile | Le gestionnaire photographie la carte grise ou la facture au garage. |
| Compression des photos | Non | Simplicité ; la taille maximale suffit. |
| Stockage | PostgreSQL, table séparée, port `IDocumentStorage` (ADR 0002) | Aucun changement de déploiement, une seule sauvegarde, volume faible. |
| Date d'expiration d'un document | Hors périmètre | Doublon avec les échéances (assurance, CT, agrément ARS), qui alertent déjà. |
| Modification d'un document | Non (supprimer puis rajouter) | Comme les interventions en phase 3 ; Could si le besoin apparaît. |
| Véhicule sorti du parc | Ajout, consultation et suppression possibles | Le certificat de cession et les factures tardives arrivent après la sortie du parc ; un document ne change pas l'état du véhicule. |
| Suppression | Définitive, sans corbeille, cascade depuis l'intervention ou la dépense | Simplicité et minimisation des données personnelles. |
| Logs | Ni contenu ni nom de fichier | Un nom de fichier peut contenir un nom de personne. |

## Hors périmètre

- Date d'expiration ou de validité sur un document (les échéances la portent).
- Lien direct entre un document et une échéance (le lien passe par l'intervention de réalisation).
- Versions, modification, corbeille, restauration d'un document.
- Recherche plein texte, reconnaissance de texte (OCR), lecture automatique des factures (montant, date).
- Documents rattachés à un problème ou à une action de résolution (photos de dégâts) : à évaluer à l'usage.
- Documents rattachés à un emprunt (le contrat de crédit se classe pour l'instant au niveau du véhicule).
- Pièces jointes des dépenses d'utilisation courante (carburant, péages).
- Vue globale des documents de tout le parc ; envoi par mail ; partage par lien.
- Conversion HEIC, compression ou redimensionnement des images, aperçu miniature.
- Changement de stockage (disque, objet) : seulement au seuil de bascule de l'ADR 0002.
- Rôles et restrictions d'accès par catégorie : phase 9.

## Impact sur le modèle de données

- **Catégorie de document** (nouveau, référentiel) : nom, active. Valeurs initiales insérées par la migration
  (création de table : pas d'accord du propriétaire requis).
- **Document** (nouveau, métadonnées) : véhicule, rattachement (véhicule, intervention ou dépense), catégorie
  (obligatoire pour un document du véhicule, absente sinon), libellé facultatif, nom du fichier d'origine, type
  (PDF, JPEG, PNG), taille en octets, date d'ajout.
- **Contenu de document** (nouveau, table séparée) : identifiant du document, octets. Accédé uniquement via le
  port `IDocumentStorage` (Application) et son adaptateur PostgreSQL (Infrastructure).
- **Intervention**, **Dépense** : inchangées ; leur suppression entraîne celle de leurs documents.
- **Réalisation d'échéance** (US8) : la réponse doit permettre au front de retrouver l'intervention créée pour y
  joindre le fichier.

## Questions ouvertes

- **Proxy devant l'API** : le VPS utilise Caddy, sans limite de taille de requête par défaut ; à revérifier si une
  directive `request_body` est configurée (hors dépôt).
- **Sauvegarde de la base** : existe-t-il une sauvegarde régulière du volume PostgreSQL de prod ? Avec les
  documents, la base devient la seule copie de certains papiers.
- **Rôles (phase 9)** : proposition à valider : lecture seule = consulter et télécharger ; gestionnaire = tout ;
  catégorie « Contrat de location ou de crédit » réservée au service financier, comme les emprunts.
- **Durée de conservation (RGPD, phase 9)** : faut-il supprimer les documents d'un véhicule sorti du parc après
  un délai (par exemple 5 ans, ou la durée de conservation comptable de 10 ans pour les factures) ?
