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

vi.mock("../../companies/api/companyQueries", () => ({
  useCompanies: () => ({
    data: [
      { id: "c1", name: "Ambulances Nord", isActive: true },
      { id: "c2", name: "Pompes Funèbres Sud", isActive: true },
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
        error={new Error("Le VIN doit comporter 17 caractères")}
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

  it("renvoie toujours ownerCompanyId, propose les sociétés actives et ne permet pas de vider le propriétaire", () => {
    const onSubmit = vi.fn();
    render(
      <VehicleFormDialog
        open
        initialValues={{ ...emptyVehicleForm, licensePlate: "AB-123-CD", ownerCompanyId: "old" }}
        currentOwner={{ id: "old", name: "Ancienne société", isActive: false }}
        submitting={false}
        error={null}
        onSubmit={onSubmit}
        onClose={vi.fn()}
      />,
    );

    const select = screen.getByRole("combobox", { name: "Société propriétaire" });
    expect(select).toHaveTextContent("Ancienne société (désactivée)");
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));
    expect(onSubmit).toHaveBeenCalledWith(expect.objectContaining({ ownerCompanyId: "old" }));

    fireEvent.mouseDown(select);
    expect(screen.getByRole("option", { name: "Ambulances Nord" })).toBeInTheDocument();
    expect(screen.queryByRole("option", { name: "À renseigner" })).not.toBeInTheDocument();
  });

  it("autorise un propriétaire vide à la création", () => {
    render(<VehicleFormDialog open submitting={false} error={null} onSubmit={vi.fn()} onClose={vi.fn()} />);
    fireEvent.mouseDown(screen.getByRole("combobox", { name: "Société propriétaire" }));
    expect(screen.getByRole("option", { name: "À renseigner" })).toBeInTheDocument();
  });

  it("véhicule sorti du parc : seul le propriétaire est éditable", () => {
    render(
      <VehicleFormDialog
        open
        ownerOnly
        initialValues={{ ...emptyVehicleForm, licensePlate: "AB-123-CD", brand: "Renault" }}
        submitting={false}
        error={null}
        onSubmit={vi.fn()}
        onClose={vi.fn()}
      />,
    );
    expect(screen.getByLabelText("Marque")).toBeDisabled();
    expect(screen.getByRole("combobox", { name: /Société propriétaire/ })).not.toHaveAttribute("aria-disabled", "true");
  });

  it("affiche l'erreur de propriétaire à côté du champ", () => {
    const error = Object.assign(new Error("x"), {
      isAxiosError: true,
      response: { data: { code: "vehicle.owner_cannot_be_removed" } },
    });
    render(
      <VehicleFormDialog open submitting={false} error={error} onSubmit={vi.fn()} onClose={vi.fn()} />,
    );
    expect(screen.getByText(/Le propriétaire ne peut pas être retiré/)).toBeInTheDocument();
  });

  it("sorti du parc : enregistrement bloqué tant que le propriétaire est vide, sans option vide", () => {
    render(
      <VehicleFormDialog
        open
        ownerOnly
        initialValues={{ ...emptyVehicleForm, licensePlate: "AB-123-CD" }}
        submitting={false}
        error={null}
        onSubmit={vi.fn()}
        onClose={vi.fn()}
      />,
    );
    expect(screen.getByRole("button", { name: "Enregistrer" })).toBeDisabled();
    expect(screen.getByText("Choisissez la société propriétaire pour enregistrer")).toBeInTheDocument();
    fireEvent.mouseDown(screen.getByRole("combobox", { name: /Société propriétaire/ }));
    expect(screen.queryByRole("option", { name: "À renseigner" })).not.toBeInTheDocument();
  });

  it("vehicle.retired : bandeau en modification normale, message du champ en mode propriétaire seul", () => {
    const error = Object.assign(new Error("x"), {
      isAxiosError: true,
      response: { data: { code: "vehicle.retired" } },
    });
    const { unmount } = render(
      <VehicleFormDialog open submitting={false} error={error} onSubmit={vi.fn()} onClose={vi.fn()} />,
    );
    expect(screen.getByText(/sorti du parc : il ne peut plus être modifié/)).toBeInTheDocument();
    unmount();
    render(
      <VehicleFormDialog
        open
        ownerOnly
        initialValues={{ ...emptyVehicleForm, licensePlate: "AB-123-CD", ownerCompanyId: "c1" }}
        submitting={false}
        error={error}
        onSubmit={vi.fn()}
        onClose={vi.fn()}
      />,
    );
    expect(screen.queryByText(/il ne peut plus être modifié/)).not.toBeInTheDocument();
    expect(screen.getByText("Choisissez la société propriétaire pour enregistrer")).toBeInTheDocument();
  });
});
