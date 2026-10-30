import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import { IconButton, Table, TableBody, TableCell, TableContainer, TableHead, TableRow } from "@mui/material";
import { formatEuro } from "../../../shared/format/formatEuro";
import { formatDateOnly } from "../../vehicles/components/vehicleForm";
import { loanEndMonth, type Loan } from "../types/loan";

interface LoanListProps {
  loans: Loan[];
  onEdit: (loan: Loan) => void;
  onDelete: (loan: Loan) => void;
}

export default function LoanList({ loans, onEdit, onDelete }: LoanListProps) {
  return (
    <TableContainer>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Organisme</TableCell>
            <TableCell align="right">Emprunté</TableCell>
            <TableCell align="right">Mensualité</TableCell>
            <TableCell>Début</TableCell>
            <TableCell align="right">Durée</TableCell>
            <TableCell>Fin (dernière mensualité)</TableCell>
            <TableCell />
          </TableRow>
        </TableHead>
        <TableBody>
          {loans.map((loan) => (
            <TableRow key={loan.id}>
              <TableCell>{loan.lender}</TableCell>
              <TableCell align="right">{formatEuro(loan.borrowedAmount)}</TableCell>
              <TableCell align="right">{formatEuro(loan.monthlyPayment)}</TableCell>
              <TableCell>{formatDateOnly(loan.startDate)}</TableCell>
              <TableCell align="right">{loan.durationMonths} mois</TableCell>
              <TableCell>{loanEndMonth(loan.startDate, loan.durationMonths)}</TableCell>
              <TableCell align="right" sx={{ whiteSpace: "nowrap" }}>
                <IconButton aria-label={`Modifier l'emprunt ${loan.lender}`} onClick={() => onEdit(loan)}>
                  <EditIcon fontSize="small" />
                </IconButton>
                <IconButton aria-label={`Supprimer l'emprunt ${loan.lender}`} onClick={() => onDelete(loan)}>
                  <DeleteIcon fontSize="small" />
                </IconButton>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}
