/**
 * Configuration applicative résolue au runtime.
 *
 * Priorité : window.__APP_CONFIG__ (généré au démarrage du conteneur, cf. docker/entrypoint)
 * > variables VITE_* (développement local via .env.local) > valeurs par défaut.
 * Une même image Docker peut ainsi être déployée en dev et en prod.
 */
export interface AppConfig {
  envName: string;
  apiUrl: string;
  geolocApiUrl: string;
  mapStyleUrl: string;
  keycloakUrl: string;
  keycloakRealm: string;
  keycloakClientId: string;
}

declare global {
  interface Window {
    __APP_CONFIG__?: Partial<AppConfig>;
  }
}

export const defaultConfig: AppConfig = {
  envName: "local",
  apiUrl: "https://notification-api.delesse.net",
  geolocApiUrl: "https://intranet.urgencesante.fr:8091/",
  mapStyleUrl: "http://85.214.12.96:8082/styles/basic-preview/style.json",
  keycloakUrl: "https://auth.ade-dev.fr/",
  keycloakRealm: "ustest",
  keycloakClientId: "us-mecanic",
};

const buildTimeConfig: Partial<AppConfig> = {
  envName: import.meta.env.VITE_ENV_NAME,
  apiUrl: import.meta.env.VITE_API_URL,
  geolocApiUrl: import.meta.env.VITE_GEOLOC_API_URL,
  mapStyleUrl: import.meta.env.VITE_MAP_STYLE_URL,
  keycloakUrl: import.meta.env.VITE_KEYCLOAK_URL,
  keycloakRealm: import.meta.env.VITE_KEYCLOAK_REALM,
  keycloakClientId: import.meta.env.VITE_KEYCLOAK_CLIENT_ID,
};

/** Fusionne les sources en ignorant les valeurs vides. */
export function resolveConfig(
  ...sources: (Partial<AppConfig> | undefined)[]
): AppConfig {
  const resolved: AppConfig = { ...defaultConfig };
  for (const source of sources) {
    for (const [key, value] of Object.entries(source ?? {})) {
      if (typeof value === "string" && value.trim() !== "") {
        resolved[key as keyof AppConfig] = value;
      }
    }
  }
  return resolved;
}

export const appConfig = resolveConfig(
  buildTimeConfig,
  typeof window !== "undefined" ? window.__APP_CONFIG__ : undefined,
);
