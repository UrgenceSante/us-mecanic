import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
vi.mock("../../vehicle-types/api/vehicleTypeQueries", () => ({
  useVehicleTypes: () => ({
    data: [
      { id: "t1", name: "Ambulance", isActive: true },
      { id: "t2", name: "VSL", isActive: true },
    ],
  }),
}));

import VehicleFormDialog from "./VehicleFormDialog";
import { emptyVehicleForm } from "./vehicleForm";

describe("VehicleFormDialog", () => {
  it("transmet les valeurs saisies à la validation", () => {
    const onSubmit = vi.fn();
    render(<VehicleFormDialog open submitting={false} error={null} onSubmit={onSubmit} onClose={vi.fn()} />);

    fireEvent.change(screen.getByLabelText(/Immatriculation/), { target: { value: "AB-123-CD" } });
    fireEvent.change(screen.getByLabelText("Marque"), { target: { value: "Renault" } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    expect(onSubmit).toHaveBeenCalledWith({ ...emptyVehicleForm, licensePlate: "AB-123-CD", brand: "Renault" });
  });

  it("bloque l'immatriculation en modification et affiche l'erreur de l'API", () => {
    render(
      <VehicleFormDialog
        open
        initialValues={{ ...emptyVehicleForm, licensePlate: "AB-123-CD" }}
        submitting={false}
        error="Le VIN doit comporter 17 caractères"
        onSubmit={vi.fn()}
        onClose={vi.fn()}
      />,
    );

    expect(screen.getByLabelText(/Immatriculation/)).toBeDisabled();
    expect(screen.getByText("Le VIN doit comporter 17 caractères")).toBeInTheDocument();
  });

  it("propose les types actifs et garde le type désactivé actuel sélectionné", () => {
    const onSubmit = vi.fn();
    render(
      <VehicleFormDialog
        open
        initialValues={{ ...emptyVehicleForm, licensePlate: "AB-123-CD", typeId: "old" }}
        currentType={{ id: "old", name: "Sang" }}
        submitting={false}
        error={null}
        onSubmit={onSubmit}
        onClose={vi.fn()}
      />,
    );

    expect(screen.getByRole("combobox", { name: "Type" })).toHaveTextContent("Sang (désactivé)");
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));
    expect(onSubmit).toHaveBeenCalledWith(expect.objectContaining({ typeId: "old" }));

    fireEvent.mouseDown(screen.getByRole("combobox", { name: "Type" }));
    expect(screen.getByRole("option", { name: "Ambulance" })).toBeInTheDocument();
    expect(screen.getByRole("option", { name: "VSL" })).toBeInTheDocument();
  });
});
