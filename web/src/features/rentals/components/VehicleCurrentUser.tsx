import { Stack, Typography } from "@mui/material";
import PropertyDisplay from "../../../components/Utils/PropertyDisplay";
import type { Company } from "../../companies/types/company";
import type { VehicleUser, VehicleUserSource } from "../../vehicles/types/vehicle";

const sourceLabels: Record<VehicleUserSource, string> = {
  Tenant: " (locataire)",
  Owner: " (propriétaire)",
  NotSet: "",
};

const withState = (company: Company) => `${company.name}${company.isActive ? "" : " (désactivée)"}`;

/** Propriétaire et utilisateur actuel (règle R4). */
export default function VehicleCurrentUser({ owner, currentUser }: { owner: Company | null; currentUser: VehicleUser }) {
  return (
    <Stack gap={0.5}>
      {owner ? (
        <PropertyDisplay title="Propriétaire" content={withState(owner)} />
      ) : (
        <Typography color="warning.main">Propriétaire à renseigner</Typography>
      )}
      <PropertyDisplay
        title="Utilisateur actuel"
        content={currentUser.company ? `${withState(currentUser.company)}${sourceLabels[currentUser.source]}` : "Non renseigné"}
      />
    </Stack>
  );
}
