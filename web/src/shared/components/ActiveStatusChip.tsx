import ArchiveIcon from "@mui/icons-material/Archive";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import StatusChip from "./StatusChip";

interface Props {
  isActive: boolean;
  /** Accorde les libellés au féminin (« Active » / « Désactivée »), pour une catégorie par exemple. */
  feminine?: boolean;
}

/** Statut Actif / Désactivé des référentiels (types, garages, catégories…). */
export default function ActiveStatusChip({ isActive, feminine = false }: Props) {
  return isActive ? (
    <StatusChip tone="success" icon={<CheckCircleIcon />} label={feminine ? "Active" : "Actif"} />
  ) : (
    <StatusChip tone="neutral" icon={<ArchiveIcon />} label={feminine ? "Désactivée" : "Désactivé"} />
  );
}
