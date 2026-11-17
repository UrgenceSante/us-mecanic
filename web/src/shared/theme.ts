import { createTheme } from "@mui/material/styles";

const touchTarget = { minHeight: 44 };
const focusRing = (offset: number) => ({
  "&.Mui-focusVisible, &:focus-visible": { outline: "2px solid currentColor", outlineOffset: offset },
});

/** Focus clavier visible partout (WCAG 2.4.7), appliqué via GlobalStyles. */
export const globalFocusStyles = {
  ":focus-visible": { outline: "2px solid #1a237e", outlineOffset: "2px" },
};

/** Thème commun : contraste d'avertissement et cibles tactiles de 44 px. */
export const theme = createTheme({
  palette: {
    // #ed6c02 (3,11:1 avec du blanc) assombri pour atteindre 4,5:1 (WCAG 1.4.3).
    warning: { main: "#b25000" },
  },
  components: {
    // Par défaut subtitle1/2 sont rendus en h6 : ce sont des textes, pas des titres (hiérarchie des titres).
    MuiTypography: { defaultProps: { variantMapping: { subtitle1: "p", subtitle2: "p" } } },
    MuiButton: { styleOverrides: { root: { ...touchTarget, ...focusRing(2) } } },
    MuiIconButton: { styleOverrides: { root: { ...touchTarget, minWidth: 44, ...focusRing(2) } } },
    MuiTab: { styleOverrides: { root: { ...touchTarget, ...focusRing(-2) } } },
    MuiListItemButton: { styleOverrides: { root: { ...touchTarget, ...focusRing(-2) } } },
  },
});
