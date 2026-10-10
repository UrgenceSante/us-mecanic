import { isAxiosError } from "axios";

/** Corps d'erreur renvoyé par l'API (RFC 9457), avec le code métier stable. */
export interface ProblemDetails {
  title?: string;
  status?: number;
  code?: string;
}

/** Message lisible d'une erreur d'appel API : titre ProblemDetails si présent, sinon message générique. */
export function problemMessage(error: unknown, fallback = "Une erreur est survenue."): string {
  if (isAxiosError<ProblemDetails>(error)) {
    return error.response?.data?.title ?? (error.response ? fallback : "API injoignable.");
  }
  return error instanceof Error ? error.message : fallback;
}
