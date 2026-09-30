#!/bin/sh
# Génère /config.js à partir des variables d'environnement APP_* du conteneur.
# Une variable absente ou vide garde la valeur par défaut du front (src/config/appConfig.ts).
set -eu

TARGET=/usr/share/nginx/html/config.js

json_escape() {
  printf '%s' "$1" | sed -e 's/\\/\\\\/g' -e 's/"/\\"/g'
}

entry() {
  key="$1"
  value="$2"
  if [ -n "$value" ]; then
    printf '  "%s": "%s",\n' "$key" "$(json_escape "$value")"
  fi
}

{
  echo "window.__APP_CONFIG__ = {"
  entry envName "${APP_ENV_NAME:-}"
  entry apiUrl "${APP_API_URL:-}"
  entry geolocApiUrl "${APP_GEOLOC_API_URL:-}"
  entry mapStyleUrl "${APP_MAP_STYLE_URL:-}"
  entry keycloakUrl "${APP_KEYCLOAK_URL:-}"
  entry keycloakRealm "${APP_KEYCLOAK_REALM:-}"
  entry keycloakClientId "${APP_KEYCLOAK_CLIENT_ID:-}"
  echo "};"
} > "$TARGET"

echo "40-app-config: config.js généré (env=${APP_ENV_NAME:-non défini})"
