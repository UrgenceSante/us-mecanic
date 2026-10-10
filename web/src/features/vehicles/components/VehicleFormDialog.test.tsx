import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
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
});
