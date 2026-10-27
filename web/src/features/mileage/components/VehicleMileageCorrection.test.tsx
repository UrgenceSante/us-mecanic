import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import {
  AxiosError,
  type AxiosResponse,
  type InternalAxiosRequestConfig,
} from "axios";
import { beforeEach, describe, expect, it, vi } from "vitest";
import VehicleMileage from "./VehicleMileage";

const api = vi.hoisted(() => ({
  listMileageReadings: vi.fn(),
  addMileageReading: vi.fn(),
  updateMileageReading: vi.fn(),
  deleteMileageReading: vi.fn(),
}));
vi.mock("../api/mileageApi", () => api);
vi.mock("../../../hooks/useNotifSnack", () => ({
  default: () => ({ notifySuccess: vi.fn(), notifyError: vi.fn() }),
}));

function problem(code: string, title = "Erreur") {
  const response = { status: 400, data: { code, title } } as AxiosResponse;
  return new AxiosError(
    title,
    "ERR_BAD_REQUEST",
    {} as InternalAxiosRequestConfig,
    null,
    response,
  );
}

const readings = [
  {
    id: "r2",
    vehicleId: "v1",
    date: "2026-03-10",
    kilometers: 12000,
    reason: null,
    recordedAt: "",
  },
  {
    id: "r1",
    vehicleId: "v1",
    date: "2026-01-05",
    kilometers: 9000,
    reason: "Compteur changé",
    recordedAt: "",
  },
];

function renderBlock(status: "Active" | "Retired" = "Active") {
  const client = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  render(
    <QueryClientProvider client={client}>
      <VehicleMileage
        vehicleId="v1"
        status={status}
        currentMileage={12000}
        currentMileageDate="2026-03-10"
      />
    </QueryClientProvider>,
  );
}

describe("VehicleMileage : correction des relevés", () => {
  beforeEach(() => {
    vi.resetAllMocks();
    api.listMileageReadings.mockResolvedValue(readings);
  });

  it("préremplit le formulaire de modification et envoie un PUT", async () => {
    api.updateMileageReading.mockResolvedValue(readings[1]);
    renderBlock();

    fireEvent.click(
      await screen.findByRole("button", { name: "Modifier le relevé du 05/01/2026" }),
    );
    const km = await screen.findByLabelText(/Kilométrage/);
    expect(km).toHaveValue(9000);
    expect(screen.getByLabelText(/Date du relevé/)).toHaveValue("2026-01-05");
    expect(screen.getByLabelText(/Motif/)).toHaveValue("Compteur changé");

    fireEvent.change(km, { target: { value: "9500" } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    await waitFor(() =>
      expect(api.updateMileageReading).toHaveBeenCalledWith("v1", "r1", {
        date: "2026-01-05",
        kilometers: 9500,
        reason: "Compteur changé",
        confirmHighMileage: false,
      }),
    );
  });

  it("applique confirmation puis remise à zéro lors d'une modification", async () => {
    api.updateMileageReading.mockRejectedValueOnce(
      problem("mileage_reading.high_mileage_confirmation_required"),
    );
    api.updateMileageReading.mockResolvedValue(readings[1]);
    renderBlock();

    fireEvent.click(
      await screen.findByRole("button", { name: "Modifier le relevé du 05/01/2026" }),
    );
    const km = await screen.findByLabelText(/Kilométrage/);
    fireEvent.change(km, { target: { value: "99999" } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));
    await screen.findByRole("button", { name: "Confirmer et enregistrer" });

    fireEvent.change(km, { target: { value: "50000" } });
    expect(
      await screen.findByRole("button", { name: "Enregistrer" }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole("button", { name: "Confirmer et enregistrer" }),
    ).not.toBeInTheDocument();
  });

  it("exige un motif en modification quand l'API le demande", async () => {
    api.updateMileageReading.mockRejectedValueOnce(
      problem("mileage_reading.reason_required"),
    );
    renderBlock();

    fireEvent.click(
      await screen.findByRole("button", { name: "Modifier le relevé du 10/03/2026" }),
    );
    fireEvent.change(await screen.findByLabelText(/Kilométrage/), {
      target: { value: "100" },
    });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    expect(
      await screen.findByText(/un motif est obligatoire/),
    ).toBeInTheDocument();
  });

  it("supprime un relevé après confirmation", async () => {
    api.deleteMileageReading.mockResolvedValue(undefined);
    renderBlock();

    fireEvent.click(
      await screen.findByRole("button", { name: "Supprimer le relevé du 10/03/2026" }),
    );
    expect(api.deleteMileageReading).not.toHaveBeenCalled();
    fireEvent.click(await screen.findByRole("button", { name: "Supprimer" }));

    await waitFor(() =>
      expect(api.deleteMileageReading).toHaveBeenCalledWith("v1", "r2"),
    );
  });

  it("n'offre pas de correction sur un véhicule sorti du parc", async () => {
    renderBlock("Retired");

    await screen.findByText("05/01/2026 : 9 000 km");
    expect(
      screen.queryByRole("button", { name: /Modifier le relevé/ }),
    ).not.toBeInTheDocument();
  });
});
