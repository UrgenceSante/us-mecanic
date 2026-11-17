import { forwardRef } from "react";
import { Alert, type AlertColor } from "@mui/material";
import type { CustomContentProps } from "notistack";

/** Contenu de notification annoncé par les lecteurs d'écran : `alert` pour une erreur, `status` sinon. */
const AccessibleSnackbar = forwardRef<HTMLDivElement, CustomContentProps>(function AccessibleSnackbar(
  { message, variant },
  ref,
) {
  const severity: AlertColor = variant === "default" ? "info" : variant;
  return (
    <Alert
      ref={ref}
      variant="filled"
      severity={severity}
      role={variant === "error" ? "alert" : "status"}
      sx={{ minWidth: 240, maxWidth: 560 }}
    >
      {message}
    </Alert>
  );
});

export default AccessibleSnackbar;
