import { act, render, screen } from "@testing-library/react";
import { SnackbarProvider } from "notistack";
import { describe, expect, it } from "vitest";
import useNotifSnack from "../../hooks/useNotifSnack";
import AccessibleSnackbar from "./AccessibleSnackbar";

function Trigger({ onReady }: { onReady: (n: ReturnType<typeof useNotifSnack>) => void }) {
  onReady(useNotifSnack());
  return null;
}

function setup() {
  let notif!: ReturnType<typeof useNotifSnack>;
  render(
    <SnackbarProvider Components={{ success: AccessibleSnackbar, error: AccessibleSnackbar }}>
      <Trigger onReady={(n) => (notif = n)} />
    </SnackbarProvider>,
  );
  return notif;
}

describe("notifications accessibles", () => {
  it("annonce un succès avec role=status", () => {
    const notif = setup();
    act(() => void notif.notifySuccess("Enregistré"));
    expect(screen.getByRole("status").textContent).toContain("Enregistré");
    expect(screen.queryByRole("alert")).toBeNull();
  });

  it("annonce une erreur avec role=alert", () => {
    const notif = setup();
    act(() => void notif.notifyError("Échec"));
    expect(screen.getByRole("alert").textContent).toContain("Échec");
  });
});
