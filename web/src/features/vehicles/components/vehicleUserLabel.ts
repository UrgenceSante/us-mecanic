import type { Vehicle } from "../types/vehicle";

/** Utilisateur actuel affiché dans le parc : « Société (locataire) » en location, la société propriétaire sinon. */
export function currentUserLabel(vehicle: Pick<Vehicle, "currentUser">): string {
  const { company, source } = vehicle.currentUser;
  if (!company) return "non renseigné";
  return source === "Tenant" ? `${company.name} (locataire)` : company.name;
}
