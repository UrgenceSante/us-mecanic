import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AxiosError, type AxiosResponse, type InternalAxiosRequestConfig } from "axios";
import { beforeEach, describe, expect, it, vi } from "vitest";
import VehicleMileage from "./VehicleMileage";
import { todayLocalIso } from "../types/date";

const api = vi.hoisted(() => ({ listMileageReadings: vi.fn(), addMileageReading: vi.fn() }));
vi.mock("../api/mileageApi", () => api);
vi.mock("../../../hooks/useNotifSnack", () => ({
  default: () => ({ notifySuccess: vi.fn(), notifyError: vi.fn() }),
}));

function problem(code: string, title = "Erreur") {
  const response = { status: 400, data: { code, title } } as AxiosResponse;
  return new AxiosError(title, "ERR_BAD_REQUEST", {} as InternalAxiosRequestConfig, null, response);
}

const readings = [
  { id: "r2", vehicleId: "v1", date: "2026-03-10", kilometers: 12000, reason: null, recordedAt: "" },
  { id: "r1", vehicleId: "v1", date: "2026-01-05", kilometers: 9000, reason: "Compteur changé", recordedAt: "" },
];

function renderBlock(status: "Active" | "Retired" = "Active") {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={client}>
      <VehicleMileage vehicleId="v1" status={status} currentMileage={12000} currentMileageDate="2026-03-10" />
    </QueryClientProvider>,
  );
}

const openForm = async () => {
  fireEvent.click(await screen.findByRole("button", { name: /Saisir un relevé/ }));
  return screen.findByLabelText(/Kilométrage/);
};

describe("VehicleMileage", () => {
  beforeEach(() => {
    vi.resetAllMocks();
    api.listMileageReadings.mockResolvedValue(readings);
  });

  it("affiche le km actuel et l'historique avec les motifs", async () => {
    renderBlock();

    expect(await screen.findByText("12 000 km au 10/03/2026")).toBeInTheDocument();
    expect(await screen.findByText("Motif : Compteur changé")).toBeInTheDocument();
    expect(screen.getByText("05/01/2026 : 9 000 km")).toBeInTheDocument();
  });

  it("désactive la saisie pour un véhicule sorti du parc", async () => {
    renderBlock("Retired");

    expect(await screen.findByText(/Saisie indisponible/)).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /Saisir un relevé/ })).not.toBeInTheDocument();
  });

  it("préremplit la date du jour et envoie le relevé", async () => {
    api.addMileageReading.mockResolvedValue(readings[0]);
    renderBlock();

    fireEvent.change(await openForm(), { target: { value: "12500" } });
    expect(screen.getByLabelText(/Date du relevé/)).toHaveValue(todayLocalIso());
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    await waitFor(() =>
      expect(api.addMileageReading).toHaveBeenCalledWith("v1", {
        date: todayLocalIso(),
        kilometers: 12500,
        reason: null,
        confirmHighMileage: false,
      }),
    );
  });

  it("rend le motif obligatoire quand l'API signale un recul", async () => {
    api.addMileageReading.mockRejectedValueOnce(problem("mileage_reading.reason_required"));
    api.addMileageReading.mockResolvedValue(readings[0]);
    renderBlock();

    fireEvent.change(await openForm(), { target: { value: "100" } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));
    expect(await screen.findByText(/un motif est obligatoire/)).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));
    expect(api.addMileageReading).toHaveBeenCalledTimes(1);

    fireEvent.change(screen.getByLabelText(/Motif/), { target: { value: "Compteur remplacé" } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    await waitFor(() => expect(api.addMileageReading).toHaveBeenCalledTimes(2));
    expect(api.addMileageReading.mock.calls[1][1]).toMatchObject({ reason: "Compteur remplacé", confirmHighMileage: false });
  });

  it("demande une confirmation puis renvoie avec confirmHighMileage", async () => {
    api.addMileageReading.mockRejectedValueOnce(problem("mileage_reading.high_mileage_confirmation_required"));
    api.addMileageReading.mockResolvedValue(readings[0]);
    renderBlock();

    fireEvent.change(await openForm(), { target: { value: "99999" } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));
    fireEvent.click(await screen.findByRole("button", { name: "Confirmer et enregistrer" }));

    await waitFor(() => expect(api.addMileageReading).toHaveBeenCalledTimes(2));
    expect(api.addMileageReading.mock.calls[1][1]).toMatchObject({ kilometers: 99999, confirmHighMileage: true });
  });

  it("traduit les autres erreurs par code", async () => {
    api.addMileageReading.mockRejectedValue(problem("mileage_reading.date_in_future", "x"));
    renderBlock();

    fireEvent.change(await openForm(), { target: { value: "100" } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    expect(await screen.findByText("La date du relevé ne peut pas être dans le futur.")).toBeInTheDocument();
  });
});
