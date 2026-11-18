import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import VehicleOwnerLine from "./VehicleOwnerLine";

describe("VehicleOwnerLine", () => {
  it("affiche un propriétaire actif sans bouton", () => {
    render(<VehicleOwnerLine owner={{ id: "c1", name: "Ambulances Nord", isActive: true }} onFill={vi.fn()} />);
    expect(screen.getByText("Propriétaire : Ambulances Nord")).toBeInTheDocument();
    expect(screen.queryByRole("button")).not.toBeInTheDocument();
  });

  it("signale un propriétaire désactivé", () => {
    render(<VehicleOwnerLine owner={{ id: "c1", name: "Ancienne", isActive: false }} onFill={vi.fn()} />);
    expect(screen.getByText("Propriétaire : Ancienne (désactivée)")).toBeInTheDocument();
  });

  it("sans propriétaire : mention et bouton qui appelle onFill", () => {
    const onFill = vi.fn();
    render(<VehicleOwnerLine owner={null} onFill={onFill} />);
    expect(screen.getByText("Propriétaire à renseigner")).toBeInTheDocument();
    fireEvent.click(screen.getByRole("button", { name: "Renseigner le propriétaire" }));
    expect(onFill).toHaveBeenCalled();
  });
});
