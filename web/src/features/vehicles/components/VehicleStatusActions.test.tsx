import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import VehicleStatusActions from "./VehicleStatusActions";

describe("VehicleStatusActions", () => {
  it("demande confirmation dans un dialogue avant de sortir du parc", () => {
    const onChange = vi.fn();
    render(<VehicleStatusActions status="Active" disabled={false} onChange={onChange} />);

    fireEvent.click(screen.getByRole("button", { name: "Sortir du parc" }));
    expect(onChange).not.toHaveBeenCalled();
    expect(screen.getByRole("dialog", { name: "Sortir le véhicule du parc" })).toBeInTheDocument();
    expect(screen.getByText("Cette action est définitive.")).toBeInTheDocument();

    fireEvent.click(screen.getAllByRole("button", { name: "Sortir du parc" })[0]);
    expect(onChange).toHaveBeenCalledWith("Retired");
  });

  it("renonce à la sortie avec Annuler", () => {
    const onChange = vi.fn();
    render(<VehicleStatusActions status="Active" disabled={false} onChange={onChange} />);
    fireEvent.click(screen.getByRole("button", { name: "Sortir du parc" }));
    fireEvent.click(screen.getByRole("button", { name: "Annuler" }));
    expect(onChange).not.toHaveBeenCalled();
  });
});
