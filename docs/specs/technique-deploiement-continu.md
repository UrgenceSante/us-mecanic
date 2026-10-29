# Spec technique — Déploiement continu

Statut : en cours de cadrage (2026-10-29)

## Contexte et objectif

La CI publie déjà des images sur Docker Hub (`alexandredelesse/us-mecanic`, `alexandredelesse/us-mecanic-api`) :
`dev.yml` sur push de la branche `dev`, `release.yml` (release-please) sur `master`. Le redéploiement se fait à
la main (`deploy/redeploy.sh`).

En pratique, le travail est mergé directement sur `master` par PR et la branche `dev` n'est plus utilisée.
Objectif : que chaque merge sur `master` mette à jour l'environnement de dev sans intervention, et qu'une
release publie une version figée pour la prod.

## Environnement cible

- VPS du propriétaire, déjà doté de plusieurs stacks, d'un reverse proxy Caddy et de Watchtower.
- Caddy (routage, domaines) et Watchtower (images surveillées) sont configurés par le propriétaire, hors repo.
- Stack applicative lancée par `docker compose` (`deploy/docker-compose.yml`) : web, api, service `db`
  PostgreSQL avec volume de données. Aucune stack n'existe encore pour ce projet sur le VPS.

## Exigences

**T1 — Publication à chaque merge sur `master` (Must)**
- [ ] Un merge sur `master` lance la CI puis publie les images web et api avec les tags `dev` et `dev-<sha>`.
- [ ] Une CI rouge ne publie rien.

**T2 — Publication d'une release (Must)**
- [ ] Le merge de la PR release-please crée le tag `vX.Y.Z` et publie les images `X.Y.Z`, `X.Y` et `latest`
      (comportement existant, à conserver).

**T3 — Mise à jour sur le VPS (Must)**
- [ ] Watchtower met à jour les conteneurs surveillés quand leur tag est republié (configuration par le
      propriétaire). Le repo documente les tags à suivre : `dev` pour l'environnement de dev, version figée
      ou `latest` pour la prod.
- [ ] `deploy/redeploy.sh` reste disponible pour un déploiement manuel ou un retour arrière sur une version.

**T4 — Nettoyage (Should)**
- [ ] La branche `dev` et le workflow `dev.yml` sont retirés ; le README (« Branches et environnements »,
      « Déploiement ») décrit le nouveau flux.

## Hors périmètre

- Configuration de Caddy et de Watchtower sur le VPS (propriétaire).
- Déploiement par SSH depuis la CI.

## Points à vérifier par le propriétaire

- Secrets GitHub `DOCKERHUB_USERNAME` et `DOCKERHUB_TOKEN` : configurés en repository secrets (confirmé
  le 2026-10-29).
- PR sensible (touche `.github/workflows/`) : merge par le propriétaire, sans auto-merge.

## Questions ouvertes

- Domaines dev et prod : à fournir seulement si une configuration du repo en a besoin.
