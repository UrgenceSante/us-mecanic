# Parcours de démonstration (v1)

Parcours de 5 à 10 minutes, du point de vue d'un gestionnaire de parc d'une entreprise d'ambulances, de pompes funèbres et de transport de produits sanguins (PSL). Toutes les données sont fictives.

## 1. Préparer la démo

1. Lancer la stack avec l'authentification désactivée (dev uniquement).
   - En local :
     ```bash
     docker run -d --name us-mecanic-db -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres:16
     cd api
     Auth__Disabled=true dotnet run --project src/UsMecanic.Api   # http://localhost:5255
     ```
     Dans un autre terminal, front sans connexion Keycloak : copier `web/.env.example` en `web/.env.local` et y décommenter `VITE_AUTH_DISABLED=true`, puis `cd web && npm ci && npm run dev` (http://localhost:5173).
   - Sur l'environnement de dev : `API_AUTH_DISABLED=true` et `APP_AUTH_DISABLED=true` dans `deploy/.env.dev` (voir `deploy/.env.dev.example`), puis `deploy/redeploy.sh dev` depuis la racine du dépôt.
2. Charger les données (prérequis : bash, curl, jq, `date` GNU) :
   ```bash
   scripts/demo-seed.sh http://localhost:5255     # ou l'URL de l'API de dev
   scripts/demo-seed.sh --dry-run                 # pour voir les requêtes sans les envoyer
   ```
3. Le script **refuse une API authentifiée** (« API authentifiée : refus d'exécuter le script de démo ») : il ne peut pas tourner en prod.
4. Le script ne rejoue pas la démo : si les véhicules `DM-…` existent, il ne fait rien. Pour repartir de zéro, utiliser une base vide. La démo modifie des données (clôture d'action, étape 3) : la recharger avant chaque présentation.

Les dates sont calculées par rapport au jour d'exécution : les échéances « dépassées » le restent le jour de la démo.

## 2. Parcours conseillé

### Étape 1 — Tableau de bord (1 min)

1. Ouvrir **Tableau de bord**, onglet **Échéances**.
2. Montrer les échéances dépassées en haut de la liste, puis les proches : contrôle technique de `DM-001-AA` dépassé, assurance de `DM-006-AA` dépassée, etc.
3. Montrer le bouton **Marquer réalisée** sur une ligne.
4. Passer à l'onglet **Problèmes** : la colonne **Blocage** classe du plus au moins bloquant. Les deux problèmes **Immobilisant** (`DM-002-AA`, `DM-009-AA`) sont en tête, puis les **Non analysé** (`DM-001-AA`, `DM-004-AA`), puis les autres.

Message clé : le gestionnaire voit en un écran ce qui l'empêche de rouler et ce qui arrive à échéance.

### Étape 2 — Fiche d'une ambulance immobilisée (2 min)

1. Depuis la liste des problèmes, cliquer sur la plaque `DM-002-AA` (Mercedes-Benz Sprinter, ambulance). Le véhicule est **Immobilisé**.
2. Montrer le bandeau du problème immobilisant en haut de la fiche.
3. Parcourir les sections, dans l'ordre de la page :
   - **Kilométrage** : relevés sur 12 mois ;
   - **Échéances** : l'entretien constructeur est dépassé en kilométrage, d'autres échéances sont proches ;
   - **Carnet d'entretien** : remplacement de l'embrayage (1 180,90 €) ;
   - **Problèmes** : le problème immobilisant et ses actions (ouvert à l'étape 3) ;
   - **Dépenses** : remplacement du matelas de brancard (340 €).
4. Sur téléphone ou iPad, montrer les cartes mobiles à la place des tableaux.

Message clé : tout l'historique d'un véhicule est au même endroit, et son indisponibilité est visible dès l'ouverture de la fiche.

### Étape 3 — Détail du problème et clôture (2 min)

1. Dans la section problèmes de la fiche, ouvrir « Le brancard ne se relève plus : vérin bloqué » (ou passer par l'onglet **Problèmes** du tableau de bord).
2. Montrer le bloc **Analyse** : nature « Panne », élément « Brancard », immobilisation « Véhicule immobilisé ».
3. Montrer le tableau **Actions** : deux actions ouvertes, toutes deux en retard (diagnostic du constructeur, devis du vérin à valider avec la contrainte « Budget à valider »).
4. Cliquer sur **Clôturer** pour chaque action, puis confirmer dans « Clôturer l'action ». À la dernière clôture, le problème passe à **Résolu**.
5. Revenir sur la fiche `DM-002-AA` : le véhicule est de nouveau **En service**, le bandeau a disparu.

Message clé : les statuts sont automatiques. Le véhicule est immobilisé tant qu'un problème immobilisant reste ouvert, puis revient seul en service.

Variante : `DM-009-AA` (Kangoo, transport PSL) est aussi immobilisé, avec un groupe froid défaillant et une seule action proche. Pour illustrer une action liée à une intervention du carnet, voir le problème résolu de `DM-008-AA` (rail du plateau de chargement).

### Étape 4 — Page Coûts (2 min)

1. Ouvrir **Coûts** (ou le bouton **Voir les coûts** sur une fiche véhicule).
2. Choisir le raccourci **12 derniers mois** ou **Cette année**, en vue **Par mois**.
3. Montrer les cartes : **Total**, **Ponctuelles**, **Utilisation courante**, **Emprunts**, puis **Coût au km**, avec les kilomètres parcourus.
4. Filtrer sur un véhicule (liste **Véhicule**, ex. `DM-003-AA`, qui a un emprunt et une dépense prévue). Comparer avec **Tout le parc**.
5. Dans la fiche du véhicule, la dépense « Prévue : renouvellement du défibrillateur » porte la pastille **Prévue** (date future).
6. Pour `DM-011-AA` (sorti du parc, un seul relevé), le coût au km affiche « Non calculable » : il faut au moins deux relevés.

Message clé : le coût au kilomètre est calculé à partir des relevés, sans saisie supplémentaire. Carburant et péages arriveront avec les connecteurs (voir les limites).

### Étape 5 — Sociétés et locations (2 min)

1. Ouvrir **Parc** : trois sociétés fictives (Ambulances Démo, Pompes Funèbres Démo, Transport PSL Démo) possèdent les véhicules selon leur type. Filtrer par **Propriétaire**, y compris sur l'option des véhicules sans propriétaire (`DM-011-AA`, « Propriétaire à renseigner »). La page **Sociétés** (menu Administration) liste les trois sociétés.
2. Ouvrir la fiche de `DM-004-AA` (VSL) : la section **Propriétaire et locations** montre une location en cours, sans date de fin, à Pompes Funèbres Démo. Cliquer sur **Terminer** pour la clore à la date du jour.
3. Dans la même section, `DM-001-AA` montre une location terminée et `DM-007-AA` une location prévue. Le filtre **Utilisateur actuel** du parc suit les locations en cours.

4. Ouvrir **Coûts** et choisir une **Société propriétaire** : la synthèse ne garde que ses véhicules. Avec une **Société locataire**, seules les dépenses datées pendant ses locations sont comptées (les emprunts restent au propriétaire, et le coût au km n'est pas calculé).

Message clé : le parc d'un groupe de sociétés se pilote au même endroit, avec le propriétaire et l'utilisateur actuel de chaque véhicule, et les coûts se lisent par société.

### Étape 6 — Administration (1 min)

1. Sur ordinateur, ouvrir le menu **Administration** ; sur mobile, le tiroir latéral (section « Administration »).
2. Montrer **Types de véhicule** (Ambulance, VSL, Corbillard, Véhicule funéraire, Transport PSL), **Types d'échéance** (Désinfection, Chaîne du froid, Agrément ARS…), **Garages**, **Catégories de dépense** et **Référentiels problèmes** (natures, éléments, acteurs, types d'action, contraintes).
3. Créer ou renommer une entrée pour montrer l'adaptation au métier. Il n'y a pas de suppression, seulement une désactivation.

Message clé : l'application s'adapte au métier (ambulance, funéraire, PSL) sans développement.

## 3. Limites connues de la v1

1. Pas d'authentification ni de rôles avant la phase 9. La démo tourne avec l'authentification désactivée, réservée au dev.
2. Carburant et péages : non saisissables, ils viendront de connecteurs en phase 8.
3. Documents (carte grise, attestations, factures) : phase 6.
4. L'application chauffeur qui signale les problèmes est hors de ce dépôt ; la démo crée les signalements par l'API.
