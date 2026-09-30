import { Alert } from "@mui/material";
import type { AxiosError } from "axios";
import { type ReactNode } from "react";

interface AxiosErrorHandlerProps {
  error: AxiosError;
  custom404Render?: ReactNode;
}
export default function AxiosErrorHandler(props: AxiosErrorHandlerProps) {
  const { error, custom404Render } = props;

  if (custom404Render && error.status === 404) return custom404Render;
  return (
    <Alert sx={{ marginY: 2 }} severity="warning">
      {error.message}
    </Alert>
  );
}
