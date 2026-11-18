import { MenuItem, Stack, TextField } from "@mui/material";
import VehicleTypeSelect from "../../vehicle-types/components/VehicleTypeSelect";
import type useVehicleListFilters from "../hooks/useVehicleListFilters";
import { vehicleStatusLabels, type VehicleStatus } from "../types/vehicle";
import VehicleCompanyFilter from "./VehicleCompanyFilter";

type Filters = ReturnType<typeof useVehicleListFilters>;

/** Barre de filtres du parc : « Rechercher » pleine largeur sous `md`, puis sélecteurs à la suite (retour à la ligne). */
export default function VehicleListFilters({ values, setters }: Pick<Filters, "values" | "setters">) {
  return (
    <Stack direction="row" flexWrap="wrap" gap={2}>
      <TextField
        label="Rechercher"
        placeholder="Immatriculation, marque, modèle, n° interne"
        size="small"
        value={values.search}
        onChange={(e) => setters.setSearch(e.target.value)}
        sx={{ flex: "1 1 240px", minWidth: { xs: "100%", md: 240 } }}
      />
      <TextField
        select
        label="Statut"
        size="small"
        value={values.status}
        onChange={(e) => setters.setStatus(e.target.value as VehicleStatus | "")}
        sx={{ minWidth: 170, flex: { xs: "1 1 40%", md: "0 0 auto" } }}
      >
        <MenuItem value="">Tous</MenuItem>
        {Object.entries(vehicleStatusLabels).map(([value, label]) => (
          <MenuItem key={value} value={value}>
            {label}
          </MenuItem>
        ))}
      </TextField>
      <VehicleTypeSelect value={values.type} onChange={setters.setType} emptyLabel="Tous" includeInactive size="small" minWidth={170} />
      <VehicleCompanyFilter label="Propriétaire" value={values.owner} onChange={setters.setOwner} allowMissing />
      <VehicleCompanyFilter label="Utilisateur actuel" value={values.currentUser} onChange={setters.setCurrentUser} />
    </Stack>
  );
}
