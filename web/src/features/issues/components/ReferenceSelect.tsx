import { MenuItem, TextField } from "@mui/material";
import { useProblemReferences } from "../../problem-references/api/problemReferenceQueries";
import type { ProblemReferenceKind } from "../../problem-references/types/problemReference";

interface ReferenceSelectProps {
  kind: ProblemReferenceKind;
  label: string;
  value: string;
  onChange: (id: string) => void;
}

/** Liste déroulante d'un référentiel de problèmes : uniquement les valeurs actives. */
export default function ReferenceSelect({ kind, label, value, onChange }: ReferenceSelectProps) {
  const references = useProblemReferences(kind, false);
  return (
    <TextField
      select
      required
      fullWidth
      margin="dense"
      label={label}
      value={references.data?.some((r) => r.id === value) ? value : ""}
      onChange={(e) => onChange(e.target.value)}
    >
      {(references.data ?? []).map((r) => (
        <MenuItem key={r.id} value={r.id}>
          {r.name}
        </MenuItem>
      ))}
    </TextField>
  );
}
