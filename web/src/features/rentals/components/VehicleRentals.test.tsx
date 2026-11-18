import { fireEvent, render, screen, waitFor, within } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AxiosError, type AxiosResponse, type InternalAxiosRequestConfig } from "axios";
import { beforeEach, describe, expect, it, vi } from "vitest";
import type { Rental } from "../types/rental";
import RentalHeaderNotice from "./RentalHeaderNotice";
import VehicleRentals from "./VehicleRentals";

const api = vi.hoisted(() => ({
  listRentals: vi.fn(), addRental: vi.fn(), updateRental: vi.fn(), endRental: vi.fn(), deleteRental: vi.fn(),
}));
vi.mock("../api/rentalsApi", () => api);
vi.mock("../../companies/api/companiesApi", () => ({
  listCompanies: vi.fn().mockResolvedValue([{ id: "c2", name: "Société B", isActive: true }]),
}));
vi.mock("../../../hooks/useNotifSnack", () => ({
  default: () => ({ notifySuccess: vi.fn(), notifyError: vi.fn() }),
}));

const owner = { id: "c1", name: "Société A", isActive: true };
const tenant = { id: "c2", name: "Société B", isActive: true };
const ongoing: Rental = { id: "r1", vehicleId: "v1", tenant, startDate: "2026-02-01", endDate: null, state: "Ongoing" };
const ended: Rental = { id: "r0", vehicleId: "v1", tenant, startDate: "2025-01-01", endDate: "2025-03-31", state: "Ended" };
const planned: Rental = { id: "r2", vehicleId: "v1", tenant, startDate: "2099-01-01", endDate: null, state: "Planned" };

const baseVehicle = {
  id: "v1",
  status: "Active" as const,
  owner,
  currentUser: { company: tenant, source: "Tenant" as const },
};

function problem(code: string, title: string) {
  const response = { status: 409, data: { code, title } } as AxiosResponse;
  return new AxiosError(title, "ERR_BAD_REQUEST", {} as InternalAxiosRequestConfig, null, response);
}

function renderWith(ui: React.ReactElement) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(<QueryClientProvider client={client}>{ui}</QueryClientProvider>);
}

beforeEach(() => vi.resetAllMocks());

describe("VehicleRentals", () => {
  it("liste l'historique avec les puces d'état, de la plus récente à la plus ancienne", async () => {
    api.listRentals.mockResolvedValue([ended, ongoing, planned]);
    renderWith(<VehicleRentals vehicle={baseVehicle} />);
    expect(await screen.findByRole("heading", { name: "Propriétaire et locations" })).toBeInTheDocument();
    const rows = (await screen.findAllByRole("row")).slice(1);
    expect(rows).toHaveLength(3);
    expect(within(rows[0]).getByRole("img", { name: "État : Prévue" })).toBeInTheDocument();
    expect(within(rows[1]).getByRole("img", { name: "État : En cours" })).toBeInTheDocument();
    expect(within(rows[2]).getByRole("img", { name: "État : Terminée" })).toBeInTheDocument();
    expect(screen.getByText("Société B (locataire)")).toBeInTheDocument();
  });

  it("affiche l'état vide", async () => {
    api.listRentals.mockResolvedValue([]);
    renderWith(<VehicleRentals vehicle={baseVehicle} />);
    expect(await screen.findByText(/Aucune location/)).toBeInTheDocument();
  });

  it("désactive l'ajout sans propriétaire, avec une explication", async () => {
    api.listRentals.mockResolvedValue([]);
    renderWith(<VehicleRentals vehicle={{ ...baseVehicle, owner: null, currentUser: { company: null, source: "NotSet" } }} />);
    expect(await screen.findByRole("button", { name: "Ajouter une location" })).toBeDisabled();
    expect(screen.getByText("Renseignez d'abord le propriétaire du véhicule.")).toBeInTheDocument();
  });

  it("désactive l'ajout sur un véhicule sorti du parc mais garde les autres actions", async () => {
    api.listRentals.mockResolvedValue([ongoing]);
    renderWith(<VehicleRentals vehicle={{ ...baseVehicle, status: "Retired" }} />);
    expect(await screen.findByRole("button", { name: "Ajouter une location" })).toBeDisabled();
    expect(screen.getByText("Véhicule sorti du parc : aucune nouvelle location.")).toBeInTheDocument();
    expect(await screen.findByRole("button", { name: /Terminer la location/ })).toBeEnabled();
  });

  it("termine une location avec la date du jour par défaut", async () => {
    api.listRentals.mockResolvedValue([ongoing]);
    api.endRental.mockResolvedValue(ongoing);
    renderWith(<VehicleRentals vehicle={baseVehicle} />);
    fireEvent.click(await screen.findByRole("button", { name: /Terminer la location/ }));
    fireEvent.click(screen.getByRole("button", { name: "Terminer" }));
    await waitFor(() => expect(api.endRental).toHaveBeenCalledTimes(1));
    const [vehicleId, rentalId, endDate] = api.endRental.mock.calls[0];
    expect([vehicleId, rentalId]).toEqual(["v1", "r1"]);
    expect(endDate).toMatch(/^\d{4}-\d{2}-\d{2}$/);
  });

  it("affiche l'erreur de chevauchement à côté de la date de début", async () => {
    api.listRentals.mockResolvedValue([]);
    api.updateRental.mockRejectedValue(problem("rental.overlap", "Chevauche la location à B du 01/02/2026 au 31/03/2026"));
    api.listRentals.mockResolvedValue([ended]);
    renderWith(<VehicleRentals vehicle={baseVehicle} />);
    fireEvent.click(await screen.findByRole("button", { name: /Modifier la location/ }));
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));
    expect(await screen.findByText("Chevauche la location à B du 01/02/2026 au 31/03/2026")).toBeInTheDocument();
  });

  it("demande confirmation avant de supprimer", async () => {
    api.listRentals.mockResolvedValue([ended]);
    api.deleteRental.mockResolvedValue(undefined);
    renderWith(<VehicleRentals vehicle={baseVehicle} />);
    fireEvent.click(await screen.findByRole("button", { name: /Supprimer la location/ }));
    expect(screen.getByText(/Supprimer la location à Société B du 01\/01\/2025 au 31\/03\/2025 \? Cette action est définitive\./)).toBeInTheDocument();
    expect(api.deleteRental).not.toHaveBeenCalled();
    fireEvent.click(screen.getByRole("button", { name: "Supprimer" }));
    await waitFor(() => expect(api.deleteRental).toHaveBeenCalledWith("v1", "r0"));
  });
});

describe("RentalHeaderNotice", () => {
  it("affiche « Loué à … depuis le … »", async () => {
    api.listRentals.mockResolvedValue([ongoing]);
    renderWith(<RentalHeaderNotice vehicleId="v1" status="Active" />);
    expect(await screen.findByText("Loué à Société B depuis le 01/02/2026")).toBeInTheDocument();
    expect(screen.queryByText("Location en cours à terminer")).toBeNull();
  });

  it("signale la location à terminer sur un véhicule sorti du parc", async () => {
    api.listRentals.mockResolvedValue([ongoing]);
    renderWith(<RentalHeaderNotice vehicleId="v1" status="Retired" />);
    expect(await screen.findByText("Location en cours à terminer")).toBeInTheDocument();
  });

  it("n'affiche rien sans location en cours", async () => {
    api.listRentals.mockResolvedValue([ended, planned]);
    renderWith(<RentalHeaderNotice vehicleId="v1" status="Active" />);
    await waitFor(() => expect(api.listRentals).toHaveBeenCalled());
    expect(screen.queryByText(/Loué à/)).toBeNull();
  });
});
