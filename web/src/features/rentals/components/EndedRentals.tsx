import { Button } from "@mui/material";
import { useState } from "react";
import type { Rental } from "../types/rental";
import type { RentalActionHandlers } from "./RentalActions";
import RentalList from "./RentalList";

interface EndedRentalsProps extends RentalActionHandlers {
  rentals: Rental[];
}

const seeLabel = (n: number) => (n === 1 ? "Voir la location terminée" : `Voir les ${n} locations terminées`);

/** Locations terminées, repliées par défaut pour laisser l'urgent en haut de la fiche. */
export default function EndedRentals({ rentals, ...handlers }: EndedRentalsProps) {
  const [open, setOpen] = useState(false);
  if (rentals.length === 0) return null;
  return (
    <>
      <div>
        <Button onClick={() => setOpen((v) => !v)} aria-expanded={open}>
          {open ? "Masquer les locations terminées" : seeLabel(rentals.length)}
        </Button>
      </div>
      {open && <RentalList rentals={rentals} {...handlers} />}
    </>
  );
}
