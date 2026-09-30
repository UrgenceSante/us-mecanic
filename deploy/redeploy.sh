#!/usr/bin/env bash
# Redéploie un environnement à partir des images Docker Hub.
# Usage : ./redeploy.sh dev|prod [tag]
#   tag (optionnel) : surcharge IMAGE_TAG de .env.<env>, ex. ./redeploy.sh prod 0.2.1
set -euo pipefail

ENV_NAME="${1:-}"
TAG="${2:-}"

if [[ "$ENV_NAME" != "dev" && "$ENV_NAME" != "prod" ]]; then
  echo "Usage: $0 dev|prod [tag]" >&2
  exit 1
fi

cd "$(dirname "$0")"
ENV_FILE=".env.${ENV_NAME}"

if [[ ! -f "$ENV_FILE" ]]; then
  echo "❌ $ENV_FILE introuvable (partir de ${ENV_FILE}.example)" >&2
  exit 1
fi

export IMAGE_TAG_OVERRIDE="$TAG"
compose() {
  if [[ -n "$IMAGE_TAG_OVERRIDE" ]]; then
    IMAGE_TAG="$IMAGE_TAG_OVERRIDE" docker compose --env-file "$ENV_FILE" "$@"
  else
    docker compose --env-file "$ENV_FILE" "$@"
  fi
}

echo "📥 Pull des images ($ENV_NAME)…"
compose pull
echo "🚀 Démarrage…"
compose up -d --remove-orphans
compose ps
