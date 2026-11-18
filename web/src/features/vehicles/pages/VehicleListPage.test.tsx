import { fireEvent, render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router";
import { describe, expect, it, vi } from "vitest";

const { useVehicles } = vi.hoisted(() => ({ useVehicles: vi.fn() }));
vi.mock("../api/vehicleQueries", () => ({
  useVehicles,
  useRegisterVehicle: () => ({
    mutate: vi.fn(),
    reset: vi.fn(),
    isPending: false,
    isError: false,
  }),
}));
const vehiclesResult = {
  isLoading: false,
  isError: false,
  data: [
    {
      id: "v1",
      licensePlate: "AB-123-CD",
      internalNumber: null,
      typeName: "VSL",
      brand: "Renault",
      model: "Master",
      firstRegistrationDate: null,
      currentMileage: 1000,
      status: "Active",
    },
  ],
};
useVehicles.mockReturnValue(vehiclesResult);
// Le DataGrid réel importe du CSS que Node ne sait pas charger : on rend les cellules à la main.
vi.mock("@mui/x-data-grid", () => ({
  DataGrid: ({
    rows,
    columns,
  }: {
    rows: { id: string }[];
    columns: { field: string; renderCell?: (p: unknown) => unknown }[];
  }) => (
    <div>
      {rows.map((row) => (
        <div key={row.id}>
          {
            columns
              .find((c) => c.field === "licensePlate")
              ?.renderCell?.({
                row,
                value: (row as Record<string, unknown>).licensePlate,
              }) as never
          }
        </div>
      ))}
    </div>
  ),
}));
vi.mock("@mui/x-data-grid/locales", () => ({
  frFR: { components: { MuiDataGrid: { defaultProps: { localeText: {} } } } },
}));
vi.mock("../components/VehicleCompanyFilter", () => ({
  OWNER_MISSING: "none",
  default: ({ label, onChange, allowMissing }: { label: string; onChange: (v: string) => void; allowMissing?: boolean }) => (
    <button onClick={() => onChange(allowMissing ? "none" : "c1")}>{label}</button>
  ),
}));
vi.mock("../../vehicle-types/components/VehicleTypeSelect", () => ({
  default: () => null,
}));
vi.mock("../../../shared/hooks/useBelow", () => ({ default: () => false }));
vi.mock("../../../hooks/useNotifSnack", () => ({
  default: () => ({ notifySuccess: vi.fn() }),
}));

import VehicleListPage from "./VehicleListPage";

describe("VehicleListPage", () => {
  it("expose l'immatriculation comme vrai lien vers la fiche (accès clavier)", async () => {
    render(
      <MemoryRouter>
        <VehicleListPage />
      </MemoryRouter>,
    );
    const link = await screen.findByRole("link", { name: "AB-123-CD" });
    expect(link.getAttribute("href")).toBe("/vehicles/v1");
    expect(screen.getByRole("heading", { level: 1, name: "Parc de véhicules" })).toBeTruthy();
  });

  it("transmet les filtres propriétaire (À renseigner) et utilisateur actuel à la requête", () => {
    render(
      <MemoryRouter>
        <VehicleListPage />
      </MemoryRouter>,
    );
    fireEvent.click(screen.getByRole("button", { name: "Propriétaire" }));
    fireEvent.click(screen.getByRole("button", { name: "Utilisateur actuel" }));
    expect(useVehicles).toHaveBeenLastCalledWith(
      expect.objectContaining({
        ownerCompanyId: undefined,
        ownerMissing: true,
        currentUserCompanyId: "c1",
      }),
    );
  });
});
