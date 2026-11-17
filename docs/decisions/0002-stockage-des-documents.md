# ADR-0002 : Stockage des documents dans PostgreSQL, derrière un port

- Statut : accepté
- Date : 2026-11-17
- Décideur : agent `product`, par délégation du propriétaire (absent) ; à confirmer à son retour

## Contexte

La phase 6 (`docs/specs/phase-6-documents.md`) ajoute des pièces jointes : documents du véhicule (carte grise,
attestation d'assurance, PV de contrôle technique…) et factures des interventions et des dépenses. Il faut stocker
des fichiers PDF, JPEG et PNG de 10 Mo au plus.

Contraintes : la stack du VPS (`deploy/docker-compose.yml`) a un seul volume persistant, celui de PostgreSQL
(`db-data`) ; toute modification du déploiement demande l'accord du propriétaire ; le volume attendu est faible
(quelques centaines de documents par an, de l'ordre de 1 à 3 Mo chacun) ; certains documents contiennent des
données personnelles (carte grise, contrat) et ne doivent jamais être servis sans authentification.

## Options étudiées

1. **PostgreSQL (`bytea`)**, contenu dans une table séparée des métadonnées : aucun changement de déploiement,
   une seule sauvegarde (la base), cohérence transactionnelle entre métadonnées et contenu. En contrepartie, la
   base grossit, les sauvegardes et restaurations s'allongent, chaque téléchargement charge le fichier en mémoire
   de l'API.
2. **Disque du serveur** (volume Docker dédié) : base légère, lecture en flux. Mais nouveau volume (PR de
   déploiement), deuxième chose à sauvegarder, cohérence base / fichiers à gérer (fichiers orphelins).
3. **Stockage objet** (S3 ou compatible) : passe à l'échelle, sauvegarde gérée par le fournisseur. Mais nouveau
   service, compte et secrets à gérer, données personnelles chez un tiers (localisation, contrat) : disproportionné
   pour le volume visé.

## Décision

Option 1. Le contenu est stocké dans PostgreSQL, dans une table **séparée** des métadonnées (les listes ne
chargent jamais le contenu). L'accès passe par un **port `IDocumentStorage` de la couche Application**
(enregistrer, lire, supprimer un contenu par identifiant de document), avec un adaptateur PostgreSQL dans
Infrastructure. Les cas d'usage ne savent pas où est le contenu : passer au disque ou au stockage objet revient à
écrire un autre adaptateur et à recopier les contenus, sans toucher au front ni aux cas d'usage.

Raison principale : c'est la seule option qui ne change ni le déploiement ni la sauvegarde, pour un volume faible.

## Conséquences

- La création des tables est une migration de création : pas d'accord du propriétaire requis. Aucun changement
  de `deploy/`.
- La taille de la base suit le volume des documents ; la sauvegarde de la base couvre aussi les documents (encore
  faut-il qu'elle existe : question ouverte de la spec).
- Limites connues : un fichier est lu entièrement en mémoire à l'envoi et au téléchargement (acceptable à 10 Mo) ;
  pas de lecture partielle (reprise de téléchargement) ; une base de plusieurs Go allonge `pg_dump` et la
  restauration.
- **Seuil de bascule** vers le disque ou le stockage objet, à surveiller : plus de **5 Go** de contenus, ou une
  sauvegarde / restauration de la base qui dépasse **15 minutes**, ou un besoin de fichiers de plus de 10 Mo
  (vidéos, rapports volumineux). La bascule sera une PR de déploiement, donc soumise à l'accord du propriétaire.
- Avec un futur adaptateur hors base, l'enregistrement des métadonnées et du contenu ne sera plus dans une même
  transaction : il faudra traiter les contenus orphelins (nettoyage). Le port est conçu pour le permettre
  (contenu identifié par l'identifiant du document).
- Le contenu et le nom des fichiers ne sont jamais écrits dans les logs.
