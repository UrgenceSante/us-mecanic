export const formatKm = (km: number | null) => (km === null ? "-" : `${km.toLocaleString("fr-FR")} km`);

/** Suffixe « (n° interne) » à placer après l'immatriculation, vide sans n° interne. */
export const internalNumberSuffix = (vehicle: { internalNumber: string | null }) =>
  vehicle.internalNumber ? ` (${vehicle.internalNumber})` : "";
