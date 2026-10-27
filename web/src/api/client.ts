import axios, { type AxiosInstance } from "axios";
import keycloak from "../Keycloak/Keycloak";
import { appConfig } from "../config/appConfig";

const TIMEOUT_MS = 10000;
const TOKEN_MIN_VALIDITY_S = 60;

function createAuthenticatedClient(baseURL: string): AxiosInstance {
  const instance = axios.create({ baseURL, timeout: TIMEOUT_MS });

  instance.interceptors.request.use(async (config) => {
    if (!appConfig.authDisabled && keycloak.authenticated) {
      await keycloak.updateToken(TOKEN_MIN_VALIDITY_S);
      config.headers.Authorization = `Bearer ${keycloak.token}`;
    }
    return config;
  });

  return instance;
}

/** Client de l'API us-mecanic. */
export const apiClient = createAuthenticatedClient(appConfig.apiUrl);

/** Client de l'ancienne API, utilisé par le code historique (services/) jusqu'à sa reprise. */
const legacyClient = createAuthenticatedClient(appConfig.legacyApiUrl);

/** Client de l'API géoloc existante (certification de trajet). */
export const geoClient = createAuthenticatedClient(appConfig.geolocApiUrl);

export default legacyClient;
