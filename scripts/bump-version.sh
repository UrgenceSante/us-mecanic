#!/usr/bin/env bash
# Incrémente la version applicative (patch par défaut) partout où elle est déclarée :
# web/package.json, web/package-lock.json, api/Directory.Build.props et .release-please-manifest.json.
# Usage : scripts/bump-version.sh [patch|minor]
set -euo pipefail
cd "$(dirname "$0")/.."

part="${1:-patch}"
current="$(jq -r '.version' web/package.json)"
IFS=. read -r major minor patch <<<"$current"
case "$part" in
  patch) patch=$((patch + 1)) ;;
  minor) minor=$((minor + 1)); patch=0 ;;
  *) echo "Usage : $0 [patch|minor]" >&2; exit 1 ;;
esac
next="$major.$minor.$patch"

tmp="$(mktemp)"
jq --arg v "$next" '.version = $v' web/package.json >"$tmp" && cat "$tmp" >web/package.json
jq --arg v "$next" '.version = $v | .packages[""].version = $v' web/package-lock.json >"$tmp" && cat "$tmp" >web/package-lock.json
jq --arg v "$next" '.["."] = $v' .release-please-manifest.json >"$tmp" && cat "$tmp" >.release-please-manifest.json
rm -f "$tmp"
sed -i -E "s#<Version>[0-9]+\.[0-9]+\.[0-9]+</Version>#<Version>$next</Version>#" api/Directory.Build.props

echo "$current -> $next"
