/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_ENV_NAME?: string;
  readonly VITE_API_URL?: string;
  readonly VITE_GEOLOC_API_URL?: string;
  readonly VITE_MAP_STYLE_URL?: string;
  readonly VITE_KEYCLOAK_URL?: string;
  readonly VITE_KEYCLOAK_REALM?: string;
  readonly VITE_KEYCLOAK_CLIENT_ID?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
