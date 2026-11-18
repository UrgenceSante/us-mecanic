import ArchiveIcon from "@mui/icons-material/Archive";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import StatusChip from "./StatusChip";

/** Statut Actif / Désactivé des référentiels (types, garages, catégories…). */
export default function ActiveStatusChip({ isActive }: { isActive: boolean }) {
  return isActive ? (
    <StatusChip tone="success" icon={<CheckCircleIcon />} label="Actif" />
  ) : (
    <StatusChip tone="neutral" icon={<ArchiveIcon />} label="Désactivé" />
  );
}
