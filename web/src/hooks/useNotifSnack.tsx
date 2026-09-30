import { useSnackbar } from "notistack";

export default function useNotifSnack() {
  const { enqueueSnackbar } = useSnackbar();

  const notifySuccess = (msg: string) =>
    enqueueSnackbar(msg, { variant: "success" });
  const notifyError = (msg: string) =>
    enqueueSnackbar(msg, { variant: "error" });
  const notifyWarning = (msg: string) =>
    enqueueSnackbar(msg, { variant: "warning" });

  return { notifyError, notifySuccess, notifyWarning };
}
