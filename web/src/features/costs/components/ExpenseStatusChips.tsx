import CloudDownloadIcon from "@mui/icons-material/CloudDownload";
import EventIcon from "@mui/icons-material/Event";
import StatusChip from "../../../shared/components/StatusChip";
import { isManualExpense, type Expense } from "../types/expense";

/** Puces « Prévue » et « Importée » d'une dépense. */
export default function ExpenseStatusChips({ expense }: { expense: Expense }) {
  return (
    <>
      {expense.isPlanned && <StatusChip tone="info" icon={<EventIcon />} label="Prévue" />}
      {!isManualExpense(expense) && (
        <StatusChip tone="neutral" icon={<CloudDownloadIcon />} label="Importée" tooltip="Dépense importée, non modifiable" />
      )}
    </>
  );
}
