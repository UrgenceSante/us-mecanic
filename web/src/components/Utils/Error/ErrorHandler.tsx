import { Alert } from "@mui/material";
import { AxiosError, isAxiosError } from "axios";
import { type ReactNode } from "react";
import AxiosErrorHandler from "./AxiosErrorHandler";

interface ErrorHandlerProps {
  error: Error | AxiosError | null;
  custom404Render?: ReactNode;
  withoutStyle?: boolean;
  complementMsg?: string;
}
export default function ErrorHandler(props: ErrorHandlerProps) {
  const { error, withoutStyle, complementMsg, custom404Render } = props;

  if (!error) return null;

  if (isAxiosError(error))
    return (
      <AxiosErrorHandler error={error} custom404Render={custom404Render} />
    );

  if (!withoutStyle)
    return (
      <Alert sx={{ marginY: 2 }} severity="warning">
        {error.message}
        <div>{complementMsg || ""}</div>
      </Alert>
    );

  return (
    <div>
      {error.message}
      <div>{complementMsg || ""}</div>
    </div>
  );
}
