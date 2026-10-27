/**
 * Configuration applicative résolue au runtime.
 *
 * Priorité : window.__APP_CONFIG__ (généré au démarrage du conteneur, cf. docker/entrypoint)
 * > variables VITE_* (développement local via .env.local) > valeurs par défaut.
 * Une même image Docker peut ainsi être déployée en dev et en prod.
 */
export interface AppConfig {
  envName: string;
  /** API us-mecanic (gestion du parc). */
  apiUrl: string;
  /** Ancienne API (logs mécaniques, analyses, notifications), en attendant leur reprise par us-mecanic. */
  legacyApiUrl: string;
  geolocApiUrl: string;
  mapStyleUrl: string;
  keycloakUrl: string;
  keycloakRealm: string;
  keycloakClientId: string;
  /**
   * Mode développement sans authentification (ADR-0001) : saute la connexion Keycloak.
   * Actif uniquement si la valeur vaut exactement "true" ; tout autre cas laisse l'authentification active.
   */
  authDisabled: boolean;
}

declare global {
  interface Window {
    __APP_CONFIG__?: Partial<Record<keyof AppConfig, unknown>>;
  }
}

export const defaultConfig: AppConfig = {
  envName: "local",
  apiUrl: "http://localhost:5255",
  legacyApiUrl: "https://notification-api.delesse.net",
  geolocApiUrl: "https://intranet.urgencesante.fr:8091/",
  mapStyleUrl: "http://85.214.12.96:8082/styles/basic-preview/style.json",
  keycloakUrl: "https://auth.ade-dev.fr/",
  keycloakRealm: "ustest",
  keycloakClientId: "us-mecanic",
  authDisabled: false,
};

const buildTimeConfig: Partial<Record<keyof AppConfig, unknown>> = {
  envName: import.meta.env.VITE_ENV_NAME,
  apiUrl: import.meta.env.VITE_API_URL,
  legacyApiUrl: import.meta.env.VITE_LEGACY_API_URL,
  geolocApiUrl: import.meta.env.VITE_GEOLOC_API_URL,
  mapStyleUrl: import.meta.env.VITE_MAP_STYLE_URL,
  keycloakUrl: import.meta.env.VITE_KEYCLOAK_URL,
  keycloakRealm: import.meta.env.VITE_KEYCLOAK_REALM,
  keycloakClientId: import.meta.env.VITE_KEYCLOAK_CLIENT_ID,
  authDisabled: import.meta.env.VITE_AUTH_DISABLED,
};

/** Fusionne les sources en ignorant les valeurs vides. */
export function resolveConfig(
  ...sources: (Partial<Record<keyof AppConfig, unknown>> | undefined)[]
): AppConfig {
  const resolved: AppConfig = { ...defaultConfig };
  for (const source of sources) {
    for (const [key, value] of Object.entries(source ?? {})) {
      if (key === "authDisabled") {
        // Strict : seule la valeur "true" (ou le booléen true) désactive l'authentification.
        if (value !== undefined && value !== "") {
          resolved.authDisabled = value === true || value === "true";
        }
      } else if (typeof value === "string" && value.trim() !== "") {
        (resolved as unknown as Record<string, unknown>)[key] = value;
      }
    }
  }
  return resolved;
}

export const appConfig = resolveConfig(
  buildTimeConfig,
  typeof window !== "undefined" ? window.__APP_CONFIG__ : undefined,
);
