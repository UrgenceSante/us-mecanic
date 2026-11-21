import PropertyDisplay from "../../../components/Utils/PropertyDisplay";
import type { Company } from "../../companies/types/company";
import type { VehicleUser, VehicleUserSource } from "../../vehicles/types/vehicle";

const sourceLabels: Record<VehicleUserSource, string> = {
  Tenant: " (locataire)",
  Owner: " (propriétaire)",
  NotSet: "",
};

const withState = (company: Company) => `${company.name}${company.isActive ? "" : " (désactivée)"}`;

/** Utilisateur actuel (règle R4) ; le propriétaire est affiché dans l'en-tête de la fiche. */
export default function VehicleCurrentUser({ currentUser }: { currentUser: VehicleUser }) {
  return (
    <PropertyDisplay
      title="Utilisateur actuel"
      content={currentUser.company ? `${withState(currentUser.company)}${sourceLabels[currentUser.source]}` : "Non renseigné"}
    />
  );
}
