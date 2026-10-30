import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import { IconButton, Paper, Stack, Typography } from "@mui/material";
import { formatEuro } from "../../../shared/format/formatEuro";
import { formatDateOnly } from "../../vehicles/components/vehicleForm";
import { loanEndMonth, type Loan } from "../types/loan";

interface LoanCardsProps {
  loans: Loan[];
  onEdit: (loan: Loan) => void;
  onDelete: (loan: Loan) => void;
}

const touchTarget = { width: 44, height: 44 };

/** Rendu mobile : un emprunt par carte empilée. */
export default function LoanCards({ loans, onEdit, onDelete }: LoanCardsProps) {
  return (
    <Stack gap={1}>
      {loans.map((loan) => (
        <Paper key={loan.id} variant="outlined" sx={{ padding: 1.5 }} data-testid="loan-card">
          <Stack direction="row" alignItems="flex-start" gap={1}>
            <Stack flexGrow={1} gap={0.5} minWidth={0}>
              <Typography fontWeight="bold">{loan.lender}</Typography>
              <Typography variant="body2">
                {formatEuro(loan.borrowedAmount)} · {formatEuro(loan.monthlyPayment)}/mois
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {formatDateOnly(loan.startDate)} → {loanEndMonth(loan.startDate, loan.durationMonths)} ({loan.durationMonths} mois)
              </Typography>
            </Stack>
            <Stack direction="row" sx={{ marginTop: -0.5, marginRight: -0.5 }}>
              <IconButton sx={touchTarget} aria-label={`Modifier l'emprunt ${loan.lender}`} onClick={() => onEdit(loan)}>
                <EditIcon />
              </IconButton>
              <IconButton sx={touchTarget} aria-label={`Supprimer l'emprunt ${loan.lender}`} onClick={() => onDelete(loan)}>
                <DeleteIcon />
              </IconButton>
            </Stack>
          </Stack>
        </Paper>
      ))}
    </Stack>
  );
}
