import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import CancelledTransportModalContent from "./CancelledTransportModalContent";

describe("CancelledTransportModalContent", () => {
  it("déclenche onValidate ou onCancel selon le bouton cliqué", () => {
    const onValidate = vi.fn();
    const onCancel = vi.fn();
    render(
      <CancelledTransportModalContent onValidate={onValidate} onCancel={onCancel} />,
    );

    fireEvent.click(screen.getByRole("button", { name: "Oui" }));
    fireEvent.click(screen.getByRole("button", { name: "Non" }));

    expect(onValidate).toHaveBeenCalledOnce();
    expect(onCancel).toHaveBeenCalledOnce();
  });
});
