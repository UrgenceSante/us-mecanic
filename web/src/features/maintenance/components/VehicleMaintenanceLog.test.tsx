import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AxiosError, type AxiosResponse, type InternalAxiosRequestConfig } from "axios";
import { beforeEach, describe, expect, it, vi } from "vitest";
import VehicleMaintenanceLog from "./VehicleMaintenanceLog";
import { todayLocalIso } from "../../mileage/types/date";

const api = vi.hoisted(() => ({ listInterventions: vi.fn(), addIntervention: vi.fn(), deleteIntervention: vi.fn(), changeInterventionAmount: vi.fn() }));
const deadlinesApi = vi.hoisted(() => ({ listVehicleDeadlines: vi.fn() }));
const garagesApi = vi.hoisted(() => ({ listGarages: vi.fn(), createGarage: vi.fn() }));
vi.mock("../api/interventionsApi", () => api);
vi.mock("../../vehicle-deadlines/api/vehicleDeadlinesApi", () => deadlinesApi);
vi.mock("../../garages/api/garagesApi", () => garagesApi);
vi.mock("../../../hooks/useNotifSnack", () => ({
  default: () => ({ notifySuccess: vi.fn(), notifyError: vi.fn() }),
}));

function problem(code: string) {
  const response = { status: 400, data: { code, title: "Erreur" } } as AxiosResponse;
  return new AxiosError("Erreur", "ERR_BAD_REQUEST", {} as InternalAxiosRequestConfig, null, response);
}

const base = {
  vehicleId: "v1", deadlineTypeId: "t", intervalMonths: null, intervalKm: null, lastDoneDate: null,
  lastDoneKm: null, dueDate: null, dueKm: null, isClosed: false, status: "UpToDate",
};
const deadlines = [
  { ...base, id: "d1", typeName: "Contrôle technique", dueDate: "2027-01-01" },
  { ...base, id: "d2", typeName: "Vidange", intervalKm: 20000, dueKm: 80000 },
  { ...base, id: "d3", typeName: "Close", isClosed: true, status: null },
];
const interventions = [
  {
    id: "i1", vehicleId: "v1", date: "2026-05-01", kilometers: 70000, garage: { id: "g1", name: "Garage Dupont" },
    work: "Vidange complète", amount: 1234.5, recordedAt: "2026-05-01T10:00:00Z", deadlines: [{ id: "d2", typeName: "Vidange" }],
  },
];

function renderLog(status: "Active" | "Retired" = "Active") {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={client}>
      <VehicleMaintenanceLog vehicleId="v1" status={status} currentMileage={75000} />
    </QueryClientProvider>,
  );
}

const openDialog = async () => {
  fireEvent.click(await screen.findByRole("button", { name: "Ajouter une intervention" }));
  await screen.findByText("Échéances réalisées");
};
const fillWork = () => fireEvent.change(screen.getByLabelText(/Travaux réalisés/), { target: { value: "Freins" } });
const save = () => fireEvent.click(screen.getByRole("button", { name: /Enregistrer/ }));

describe("VehicleMaintenanceLog", () => {
  beforeEach(() => {
    vi.resetAllMocks();
    api.listInterventions.mockResolvedValue(interventions);
    deadlinesApi.listVehicleDeadlines.mockResolvedValue(deadlines);
    garagesApi.listGarages.mockResolvedValue([]);
  });

  it("affiche le carnet avec garage, travaux et échéances réalisées", async () => {
    renderLog();
    expect(await screen.findByText("Vidange complète")).toBeInTheDocument();
    expect(screen.getByText(/Garage Dupont/)).toBeInTheDocument();
    expect(screen.getByText(/70\D000 km/)).toBeInTheDocument();
    expect(screen.getByText(/1\D234,50\D€/)).toBeInTheDocument();
    expect(screen.getByText("Vidange")).toBeInTheDocument();
  });

  it("désactive la saisie pour un véhicule sorti du parc", async () => {
    renderLog("Retired");
    expect(await screen.findByText(/Saisie indisponible/)).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Ajouter une intervention" })).toBeNull();
  });

  it("préremplit date et km, ne propose que les échéances en cours et envoie l'intervention", async () => {
    api.addIntervention.mockResolvedValue(interventions[0]);
    renderLog();
    await openDialog();
    expect(screen.getByLabelText(/Date de l'intervention/)).toHaveValue(todayLocalIso());
    expect(screen.getByLabelText(/Kilométrage/)).toHaveValue(75000);
    expect(screen.queryByLabelText("Close")).toBeNull();

    fillWork();
    fireEvent.click(screen.getByLabelText("Contrôle technique"));
    save();

    await waitFor(() => expect(api.addIntervention).toHaveBeenCalled());
    expect(api.addIntervention).toHaveBeenCalledWith("v1", {
      date: todayLocalIso(), kilometers: 75000, garageId: null, work: "Freins", amount: null,
      deadlineIds: ["d1"], reason: null, confirmHighMileage: false,
    });
  });

  it("envoie le montant saisi avec une virgule, et bloque un montant invalide", async () => {
    api.addIntervention.mockResolvedValue(interventions[0]);
    renderLog();
    await openDialog();
    fillWork();
    const amount = screen.getByLabelText(/Montant/);
    expect(amount).toHaveAttribute("inputmode", "decimal");
    fireEvent.change(amount, { target: { value: "-3" } });
    expect(screen.getByText("Le montant ne peut pas être négatif.")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /Enregistrer/ })).toBeDisabled();
    fireEvent.change(amount, { target: { value: "89,9" } });
    save();
    await waitFor(() => expect(api.addIntervention).toHaveBeenCalled());
    expect(api.addIntervention.mock.calls[0][1]).toMatchObject({ amount: 89.9 });
  });

  it("exige les travaux et le km si une échéance cochée a un critère km", async () => {
    renderLog();
    await openDialog();
    expect(screen.getByRole("button", { name: "Enregistrer" })).toBeDisabled();
    fillWork();
    expect(screen.getByRole("button", { name: "Enregistrer" })).toBeEnabled();

    fireEvent.change(screen.getByLabelText(/Kilométrage/), { target: { value: "" } });
    fireEvent.click(screen.getByLabelText("Vidange"));
    expect(screen.getByRole("button", { name: "Enregistrer" })).toBeDisabled();
    expect(screen.getByText(/critère kilométrique/)).toBeInTheDocument();
  });

  it("demande un motif puis une confirmation, remise à zéro si le km change", async () => {
    api.addIntervention
      .mockRejectedValueOnce(problem("mileage_reading.reason_required"))
      .mockRejectedValueOnce(problem("mileage_reading.high_mileage_confirmation_required"))
      .mockResolvedValue(interventions[0]);
    renderLog();
    await openDialog();
    fillWork();
    save();
    expect(await screen.findByText(/un motif est obligatoire/)).toBeInTheDocument();

    fireEvent.change(screen.getByLabelText(/Motif/), { target: { value: "Compteur changé" } });
    save();
    expect(await screen.findByText(/Plus de 1 000 km\/jour/)).toBeInTheDocument();

    fireEvent.change(screen.getByLabelText(/Kilométrage/), { target: { value: "75100" } });
    await waitFor(() => expect(screen.queryByText(/Plus de 1 000 km\/jour/)).toBeNull());
  });

  it("supprime une intervention après confirmation, avec l'avertissement sur les échéances et le relevé", async () => {
    api.deleteIntervention.mockResolvedValue(undefined);
    renderLog();
    fireEvent.click(await screen.findByRole("button", { name: /Supprimer l'intervention du/ }));
    expect(
      screen.getByText(
        "Les échéances réalisées par cette intervention ne seront pas recalculées et le relevé kilométrique est conservé.",
      ),
    ).toBeInTheDocument();
    expect(api.deleteIntervention).not.toHaveBeenCalled();

    fireEvent.click(screen.getByRole("button", { name: "Supprimer" }));

    await waitFor(() => expect(api.deleteIntervention).toHaveBeenCalledWith("v1", "i1"));
    await waitFor(() => expect(screen.queryByText("Supprimer l'intervention")).toBeNull());
    await waitFor(() => expect(api.listInterventions.mock.calls.length).toBeGreaterThan(1));
  });

  it("n'appelle pas l'API si la suppression est annulée, et affiche l'erreur en cas d'échec", async () => {
    api.deleteIntervention.mockRejectedValue(problem("intervention.not_found"));
    renderLog();
    fireEvent.click(await screen.findByRole("button", { name: /Supprimer l'intervention du/ }));
    fireEvent.click(screen.getByRole("button", { name: "Annuler" }));
    await waitFor(() => expect(screen.queryByText("Supprimer l'intervention")).toBeNull());
    expect(api.deleteIntervention).not.toHaveBeenCalled();

    fireEvent.click(screen.getByRole("button", { name: /Supprimer l'intervention du/ }));
    fireEvent.click(screen.getByRole("button", { name: "Supprimer" }));
    expect(await screen.findByText("Cette intervention n'existe plus.")).toBeInTheDocument();
  });

  it("masque la suppression pour un véhicule sorti du parc", async () => {
    renderLog("Retired");
    expect(await screen.findByText("Vidange complète")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /Supprimer l'intervention du/ })).toBeNull();
  });

  it("modifie le montant d'une intervention, prérempli, avec une virgule", async () => {
    api.changeInterventionAmount.mockResolvedValue({ ...interventions[0], amount: 89.9 });
    renderLog();
    fireEvent.click(await screen.findByRole("button", { name: /Modifier le montant de l'intervention du/ }));
    const field = screen.getByLabelText(/Montant/);
    expect(field).toHaveValue("1234,5");

    fireEvent.change(field, { target: { value: "89,9" } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    await waitFor(() => expect(api.changeInterventionAmount).toHaveBeenCalledWith("v1", "i1", 89.9));
    await waitFor(() => expect(screen.queryByText("Modifier le montant")).toBeNull());
    expect(api.listInterventions).toHaveBeenCalledTimes(2);
  });

  it("vide le montant quand le champ est vide et bloque une saisie invalide", async () => {
    api.changeInterventionAmount.mockResolvedValue({ ...interventions[0], amount: null });
    renderLog();
    fireEvent.click(await screen.findByRole("button", { name: /Modifier le montant de l'intervention du/ }));
    const field = screen.getByLabelText(/Montant/);

    fireEvent.change(field, { target: { value: "-2" } });
    expect(screen.getByRole("button", { name: "Enregistrer" })).toBeDisabled();
    fireEvent.change(field, { target: { value: "" } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    await waitFor(() => expect(api.changeInterventionAmount).toHaveBeenCalledWith("v1", "i1", null));
  });

  it("traduit l'erreur de l'API et garde le dialogue ouvert", async () => {
    api.changeInterventionAmount.mockRejectedValue(problem("intervention.amount_too_large"));
    renderLog();
    fireEvent.click(await screen.findByRole("button", { name: /Modifier le montant de l'intervention du/ }));
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    expect(await screen.findByText("Le montant dépasse la valeur maximale autorisée.")).toBeInTheDocument();
    expect(screen.getByLabelText(/Montant/)).toBeInTheDocument();
  });

  it("masque la modification du montant et la suppression pour un véhicule sorti du parc", async () => {
    renderLog("Retired");
    expect(await screen.findByText("Vidange complète")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /Modifier le montant de l'intervention du/ })).toBeNull();
    expect(screen.queryByRole("button", { name: /Supprimer l'intervention/ })).toBeNull();
  });
});

describe("VehicleMaintenanceLog : montant vide", () => {
  beforeEach(() => {
    vi.resetAllMocks();
    deadlinesApi.listVehicleDeadlines.mockResolvedValue(deadlines);
    garagesApi.listGarages.mockResolvedValue([]);
  });

  it("propose « Ajouter un montant » quand le montant est vide, sauf véhicule sorti du parc", async () => {
    api.listInterventions.mockResolvedValue([{ ...interventions[0], amount: null }]);
    renderLog();
    fireEvent.click(await screen.findByRole("button", { name: "Ajouter un montant" }));
    expect(screen.getByLabelText(/Montant/)).toHaveValue("");
  });

  it("ne propose pas « Ajouter un montant » pour un véhicule sorti du parc", async () => {
    api.listInterventions.mockResolvedValue([{ ...interventions[0], amount: null }]);
    renderLog("Retired");
    await screen.findByText("Vidange complète");
    expect(screen.queryByRole("button", { name: "Ajouter un montant" })).toBeNull();
  });
});
