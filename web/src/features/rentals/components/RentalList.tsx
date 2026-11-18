import { Stack, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Typography } from "@mui/material";
import ActionsHeaderCell from "../../../shared/components/ActionsHeaderCell";
import ListCard from "../../../shared/components/ListCard";
import useBelow from "../../../shared/hooks/useBelow";
import { formatDateOnly } from "../../vehicles/components/vehicleForm";
import { sortRentals, type Rental } from "../types/rental";
import RentalActions, { type RentalActionHandlers } from "./RentalActions";
import RentalStateChip from "./RentalStateChip";
import { rentalPeriod } from "./rentalFormat";

interface RentalListProps extends RentalActionHandlers {
  rentals: Rental[];
}

/** Historique des locations : tableau, ou cartes empilées sur mobile. */
export default function RentalList({ rentals, ...handlers }: RentalListProps) {
  const mobile = useBelow("sm");
  const sorted = sortRentals(rentals);

  if (mobile) {
    return (
      <Stack gap={1} role="list" aria-label="Locations">
        {sorted.map((rental) => (
          <ListCard key={rental.id}>
            <Stack gap={0.5}>
              <Stack direction="row" alignItems="center" justifyContent="space-between" gap={1}>
                <Typography fontWeight="bold">{rental.tenant.name}</Typography>
                <RentalStateChip state={rental.state} />
              </Stack>
              <Typography variant="body2" color="text.secondary">
                {rentalPeriod(rental)}
              </Typography>
              <RentalActions rental={rental} {...handlers} />
            </Stack>
          </ListCard>
        ))}
      </Stack>
    );
  }

  return (
    <TableContainer>
      <Table size="small" aria-label="Locations">
        <TableHead>
          <TableRow>
            <TableCell>Locataire</TableCell>
            <TableCell>Du</TableCell>
            <TableCell>Au</TableCell>
            <TableCell>État</TableCell>
            <ActionsHeaderCell />
          </TableRow>
        </TableHead>
        <TableBody>
          {sorted.map((rental) => (
            <TableRow key={rental.id}>
              <TableCell>{rental.tenant.name}</TableCell>
              <TableCell>{formatDateOnly(rental.startDate)}</TableCell>
              <TableCell>{rental.endDate ? formatDateOnly(rental.endDate) : "-"}</TableCell>
              <TableCell>
                <RentalStateChip state={rental.state} />
              </TableCell>
              <TableCell align="right" sx={{ whiteSpace: "nowrap" }}>
                <RentalActions rental={rental} {...handlers} />
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}
