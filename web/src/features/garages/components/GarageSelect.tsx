import { Autocomplete, TextField } from "@mui/material";
import { useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { garageErrorCode, garageErrorMessage } from "../api/garageErrors";
import { garageKeys, useCreateGarage, useGarages } from "../api/garageQueries";
import { listGarages } from "../api/garagesApi";
import type { Garage } from "../types/garage";

export interface GarageSelectProps {
  /** Id du garage sélectionné, null pour aucun. */
  value: string | null;
  onChange: (garageId: string | null) => void;
  label?: string;
  /** Garage actuel d'une intervention existante, conservé affiché même désactivé. */
  current?: { id: string; name: string } | null;
  disabled?: boolean;
  size?: "small" | "medium";
}

const sameName = (a: string, b: string) => a.localeCompare(b, "fr", { sensitivity: "accent" }) === 0;

/**
 * Sélecteur de garage : propose les garages actifs ; un nom inconnu validé (Entrée) crée le garage
 * puis le sélectionne. Si le nom existe déjà côté serveur (409), le garage existant est sélectionné.
 */
export default function GarageSelect(props: GarageSelectProps) {
  const { value, onChange, label = "Garage", current, disabled, size } = props;
  const queryClient = useQueryClient();
  const garages = useGarages(false);
  const create = useCreateGarage();
  const [error, setError] = useState<string | null>(null);

  const options: Garage[] = [...(garages.data ?? [])];
  if (current && !options.some((g) => g.id === current.id)) {
    options.unshift({ id: current.id, name: `${current.name} (désactivé)`, isActive: false });
  }
  const selected = options.find((g) => g.id === value) ?? null;

  const select = (garage: Garage) => {
    setError(null);
    onChange(garage.id);
  };

  const resolveExisting = async (name: string) => {
    await queryClient.invalidateQueries({ queryKey: garageKeys.all });
    const fresh = await queryClient.fetchQuery({
      queryKey: garageKeys.list(false),
      queryFn: () => listGarages(false),
    });
    return fresh.find((g) => sameName(g.name, name));
  };

  const createAndSelect = (name: string) =>
    create.mutate(name, {
      onSuccess: select,
      onError: async (err) => {
        if (garageErrorCode(err) === "garage.name_already_exists") {
          const existing = await resolveExisting(name).catch(() => undefined);
          if (existing) return select(existing);
        }
        setError(garageErrorMessage(err));
      },
    });

  const handleChange = (_: unknown, newValue: string | Garage | null) => {
    if (newValue === null) {
      setError(null);
      onChange(null);
    } else if (typeof newValue === "string") {
      const name = newValue.trim();
      if (name === "") return;
      const known = options.find((g) => g.isActive && sameName(g.name, name));
      if (known) select(known);
      else createAndSelect(name);
    } else {
      select(newValue);
    }
  };

  return (
    <Autocomplete<Garage, false, false, true>
      freeSolo
      options={options}
      value={selected}
      disabled={disabled || create.isPending}
      loading={garages.isLoading}
      size={size}
      getOptionLabel={(o) => (typeof o === "string" ? o : o.name)}
      isOptionEqualToValue={(a, b) => a.id === b.id}
      onChange={handleChange}
      renderInput={(params) => (
        <TextField
          {...params}
          label={label}
          error={error !== null}
          helperText={error ?? "Entrée pour créer un nouveau garage"}
        />
      )}
    />
  );
}
