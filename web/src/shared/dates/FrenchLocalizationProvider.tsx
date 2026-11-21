import { LocalizationProvider } from "@mui/x-date-pickers";
import { AdapterDateFns } from "@mui/x-date-pickers/AdapterDateFns";
import { fr } from "date-fns/locale";
import { frFR } from "@mui/x-date-pickers/locales";
import type { ReactNode } from "react";

/** Fournisseur unique des sélecteurs de date (JJ/MM/AAAA, textes français) : à placer à la racine de l'application. */
export default function FrenchLocalizationProvider({ children }: { children: ReactNode }) {
  return (
    <LocalizationProvider
      dateAdapter={AdapterDateFns}
      adapterLocale={fr}
      localeText={frFR.components.MuiLocalizationProvider.defaultProps.localeText}
    >
      {children}
    </LocalizationProvider>
  );
}
