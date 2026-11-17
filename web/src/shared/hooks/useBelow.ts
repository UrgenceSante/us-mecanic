import { useMediaQuery, useTheme } from "@mui/material";
import type { Breakpoint } from "@mui/material";

/** Vrai quand l'écran est strictement plus étroit que le breakpoint (ex. `useBelow("sm")` : mobile). */
export default function useBelow(breakpoint: Breakpoint): boolean {
  return useMediaQuery(useTheme().breakpoints.down(breakpoint));
}
