import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AxiosError, type AxiosResponse, type InternalAxiosRequestConfig } from "axios";
import { beforeEach, describe, expect, it, vi } from "vitest";
import VehicleDeadlines from "./VehicleDeadlines";
import { todayLocalIso } from "../../mileage/types/date";

const api = vi.hoisted(() => ({
  listVehicleDeadlines: vi.fn(),
  addVehicleDeadline: vi.fn(),
  completeVehicleDeadline: vi.fn(),
}));
const garagesApi = vi.hoisted(() => ({ listGarages: vi.fn(), createGarage: vi.fn() }));
const typesApi = vi.hoisted(() => ({ listDeadlineTypes: vi.fn() }));
vi.mock("../api/vehicleDeadlinesApi", () => api);
vi.mock("../../garages/api/garagesApi", () => garagesApi);
vi.mock("../../deadline-types/api/deadlineTypesApi", () => typesApi);
vi.mock("../../../hooks/useNotifSnack", () => ({
  default: () => ({ notifySuccess: vi.fn(), notifyError: vi.fn() }),
}));

function problem(code: string, title = "Erreur") {
  const response = { status: 400, data: { code, title } } as AxiosResponse;
  return new AxiosError(title, "ERR_BAD_REQUEST", {} as InternalAxiosRequestConfig, null, response);
}

const base = {
  vehicleId: "v1",
  deadlineTypeId: "t1",
  intervalMonths: null,
  intervalKm: null,
  lastDoneDate: null,
  lastDoneKm: null,
  dueDate: null,
  dueKm: null,
  isClosed: false,
};
const deadlines = [
  { ...base, id: "d1", typeName: "Contrôle technique", intervalMonths: 12, dueDate: "2026-12-01", lastDoneDate: "2025-12-01", status: "DueSoon" },
  { ...base, id: "d2", typeName: "Vidange", intervalKm: 20000, dueKm: 80000, lastDoneKm: 60000, status: "Overdue" },
  { ...base, id: "d3", typeName: "Agrément ARS", isClosed: true, status: null },
];
const types = [
  { id: "t1", name: "Contrôle technique", defaultIntervalMonths: 12, defaultIntervalKm: null, dueSoonDays: null, dueSoonKm: null, isOneOff: false, isActive: true },
  { id: "t2", name: "Extincteur", defaultIntervalMonths: null, defaultIntervalKm: null, dueSoonDays: null, dueSoonKm: null, isOneOff: true, isActive: true },
];

function renderBlock(currentMileage: number | null = 75000) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={client}>
      <VehicleDeadlines vehicleId="v1" status="Active" currentMileage={currentMileage} />
    </QueryClientProvider>,
  );
}

const complete = async (name: string) =>
  fireEvent.click(await screen.findByRole("button", { name: `Marquer réalisée : ${name}` }));

describe("VehicleDeadlines", () => {
  beforeEach(() => {
    vi.resetAllMocks();
    api.listVehicleDeadlines.mockResolvedValue(deadlines);
    typesApi.listDeadlineTypes.mockResolvedValue(types);
    garagesApi.listGarages.mockResolvedValue([{ id: "g1", name: "Garage Dupont", isActive: true }]);
  });

  it("affiche statuts, prochaine échéance et dernière réalisation", async () => {
    renderBlock();

    expect(await screen.findByText("Proche")).toBeInTheDocument();
    expect(screen.getByText("Dépassée")).toBeInTheDocument();
    expect(screen.getByText("Close")).toBeInTheDocument();
    expect(screen.getByText(/Prochaine : 01\/12\/2026 \| Dernière réalisation : 01\/12\/2025/)).toBeInTheDocument();
    expect(screen.getByText(/Prochaine : 80 000 km/)).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /Marquer réalisée : Agrément/ })).not.toBeInTheDocument();
  });

  it("préremplit la périodicité du type puis envoie l'échéance", async () => {
    api.addVehicleDeadline.mockResolvedValue(deadlines[0]);
    renderBlock();

    fireEvent.click(await screen.findByRole("button", { name: "Ajouter une échéance" }));
    fireEvent.mouseDown(await screen.findByLabelText(/Type d'échéance/));
    fireEvent.click(await screen.findByRole("option", { name: "Contrôle technique" }));
    expect(screen.getByLabelText("Périodicité (mois)")).toHaveValue(12);
    fireEvent.change(screen.getByLabelText("Dernière réalisation (date)"), { target: { value: "2026-01-10" } });
    fireEvent.click(screen.getByRole("button", { name: "Ajouter" }));

    await waitFor(() =>
      expect(api.addVehicleDeadline).toHaveBeenCalledWith("v1", {
        deadlineTypeId: "t1",
        intervalMonths: 12,
        intervalKm: null,
        lastDoneDate: "2026-01-10",
        lastDoneKm: null,
        dueDate: null,
        dueKm: null,
      }),
    );
  });

  it("propose date et km d'échéance pour une échéance ponctuelle", async () => {
    renderBlock();

    fireEvent.click(await screen.findByRole("button", { name: "Ajouter une échéance" }));
    fireEvent.mouseDown(await screen.findByLabelText(/Type d'échéance/));
    fireEvent.click(await screen.findByRole("option", { name: "Extincteur" }));

    expect(screen.getByLabelText("Date d'échéance")).toBeInTheDocument();
    expect(screen.queryByLabelText("Dernière réalisation (date)")).not.toBeInTheDocument();
  });

  it("préremplit date du jour et km actuel, km obligatoire avec critère km", async () => {
    api.completeVehicleDeadline.mockResolvedValue(deadlines[1]);
    renderBlock();

    await complete("Vidange");
    expect(screen.getByLabelText(/Date de réalisation/)).toHaveValue(todayLocalIso());
    expect(screen.getByLabelText(/Kilométrage/)).toHaveValue(75000);
    expect(screen.getByLabelText(/Kilométrage/)).toBeRequired();
    fireEvent.change(screen.getByLabelText(/Commentaire/), { target: { value: "RAS" } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    await waitFor(() =>
      expect(api.completeVehicleDeadline).toHaveBeenCalledWith("v1", "d2", {
        date: todayLocalIso(),
        kilometers: 75000,
        garageId: null,
        amount: null,
        comment: "RAS",
        reason: null,
        confirmHighMileage: false,
      }),
    );
  });

  it("envoie le montant saisi avec une virgule, et bloque un montant invalide", async () => {
    api.completeVehicleDeadline.mockResolvedValue(deadlines[1]);
    renderBlock();

    await complete("Vidange");
    const amount = screen.getByLabelText(/Montant/);
    expect(amount).toHaveAttribute("inputmode", "decimal");
    fireEvent.change(amount, { target: { value: "12,345" } });
    expect(screen.getByText("Le montant est limité à 2 décimales.")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Enregistrer" })).toBeDisabled();
    fireEvent.change(amount, { target: { value: "0" } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    await waitFor(() => expect(api.completeVehicleDeadline).toHaveBeenCalled());
    expect(api.completeVehicleDeadline.mock.calls[0][2]).toMatchObject({ amount: 0 });
  });

  it("km facultatif sans critère km", async () => {
    renderBlock(null);

    await complete("Contrôle technique");
    expect(screen.getByLabelText(/Kilométrage/)).not.toBeRequired();
  });

  it("gère le motif requis puis la confirmation", async () => {
    api.completeVehicleDeadline.mockRejectedValueOnce(problem("mileage_reading.reason_required"));
    api.completeVehicleDeadline.mockRejectedValueOnce(problem("mileage_reading.high_mileage_confirmation_required"));
    api.completeVehicleDeadline.mockResolvedValue(deadlines[1]);
    renderBlock();

    await complete("Vidange");
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));
    expect(await screen.findByText(/un motif est obligatoire/)).toBeInTheDocument();

    fireEvent.change(screen.getByLabelText(/Motif/), { target: { value: "Compteur remplacé" } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));
    fireEvent.click(await screen.findByRole("button", { name: "Confirmer et enregistrer" }));

    await waitFor(() => expect(api.completeVehicleDeadline).toHaveBeenCalledTimes(3));
    expect(api.completeVehicleDeadline.mock.calls[2][2]).toMatchObject({
      reason: "Compteur remplacé",
      confirmHighMileage: true,
    });
  });

  it("traduit kilometers_required", async () => {
    api.completeVehicleDeadline.mockRejectedValue(problem("deadline_completion.kilometers_required"));
    renderBlock();

    await complete("Vidange");
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    expect(await screen.findByText(/le kilométrage de réalisation est obligatoire/)).toBeInTheDocument();
  });

  it("envoie le garage choisi", async () => {
    api.completeVehicleDeadline.mockResolvedValue(deadlines[1]);
    renderBlock();

    await complete("Vidange");
    fireEvent.mouseDown(screen.getByLabelText("Garage"));
    fireEvent.click(await screen.findByRole("option", { name: "Garage Dupont" }));
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    await waitFor(() => expect(api.completeVehicleDeadline).toHaveBeenCalled());
    expect(api.completeVehicleDeadline.mock.calls[0][2]).toMatchObject({ garageId: "g1" });
  });

  it("vide le km prérempli quand la date n'est plus aujourd'hui", async () => {
    renderBlock();

    await complete("Vidange");
    expect(screen.getByLabelText(/Kilométrage/)).toHaveValue(75000);
    fireEvent.change(screen.getByLabelText(/Date de réalisation/), { target: { value: "2026-01-10" } });
    expect(screen.getByLabelText(/Kilométrage/)).toHaveValue(null);
  });

  it("traduit un garage désactivé", async () => {
    api.completeVehicleDeadline.mockRejectedValue(problem("intervention.garage_inactive"));
    renderBlock();

    await complete("Vidange");
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    expect(await screen.findByText(/Ce garage est désactivé/)).toBeInTheDocument();
  });
});
