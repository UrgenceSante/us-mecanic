import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AxiosError, type AxiosResponse, type InternalAxiosRequestConfig } from "axios";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { todayLocal } from "./expenseForm";
import { expensesTotal } from "../types/expense";
import VehicleExpenses from "./VehicleExpenses";

const api = vi.hoisted(() => ({ listExpenses: vi.fn(), addExpense: vi.fn(), updateExpense: vi.fn(), deleteExpense: vi.fn() }));
const cats = vi.hoisted(() => ({ listExpenseCategories: vi.fn() }));
vi.mock("../api/expensesApi", () => api);
vi.mock("../api/expenseCategoriesApi", () => cats);
vi.mock("../../../hooks/useNotifSnack", () => ({
  default: () => ({ notifySuccess: vi.fn(), notifyError: vi.fn() }),
}));

function problem(code: string) {
  const response = { status: 409, data: { code, title: "Erreur" } } as AxiosResponse;
  return new AxiosError("Erreur", "ERR_BAD_REQUEST", {} as InternalAxiosRequestConfig, null, response);
}

const base = {
  vehicleId: "v1", family: "OneOff", categoryId: "c1", categoryName: "Lavage", label: "Lavage complet",
  source: "Manual", recordedAt: "2026-05-01T10:00:00Z", isPlanned: false,
};
const manual = { ...base, id: "e1", date: "2026-05-10", amount: 25.5 };
const planned = { ...base, id: "e2", date: "2099-01-01", amount: 100, label: "Contrôle", isPlanned: true };
const fuel = { ...base, id: "e3", date: "2026-05-02", amount: 60, source: "Total", family: "Running", label: "Carburant" };

function renderExpenses(status: "Active" | "Retired" = "Active") {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={client}>
      <VehicleExpenses vehicleId="v1" status={status} />
    </QueryClientProvider>,
  );
}

beforeEach(() => {
  vi.resetAllMocks();
  api.listExpenses.mockResolvedValue([manual, planned, fuel]);
  cats.listExpenseCategories.mockResolvedValue([
    { id: "c1", name: "Lavage", isActive: true },
    { id: "c2", name: "Parking", isActive: true },
    { id: "c3", name: "Ancienne", isActive: false },
  ]);
});

async function fillValidForm() {
  fireEvent.click(await screen.findByRole("button", { name: "Ajouter une dépense" }));
  fireEvent.change(screen.getByLabelText(/^Date/), { target: { value: "2026-06-01" } });
  fireEvent.mouseDown(screen.getByRole("combobox", { name: /Catégorie/ }));
  expect(screen.queryByRole("option", { name: /Ancienne/ })).toBeNull();
  fireEvent.click(await screen.findByRole("option", { name: "Parking" }));
}

describe("expensesTotal", () => {
  it("somme au centime près", () => {
    expect(expensesTotal([{ ...manual, amount: 0.1 }, { ...manual, amount: 0.2 }] as never)).toBe(0.3);
  });
});

describe("rendu mobile", () => {
  it("affiche des cartes empilées sous le breakpoint sm", async () => {
    window.matchMedia = vi.fn().mockImplementation((query: string) => ({
      matches: true, media: query, addEventListener: vi.fn(), removeEventListener: vi.fn(),
      addListener: vi.fn(), removeListener: vi.fn(), dispatchEvent: vi.fn(), onchange: null,
    }));
    try {
      renderExpenses();
      expect(await screen.findAllByTestId("expense-card")).toHaveLength(3);
      expect(screen.queryByRole("table")).toBeNull();
      expect(screen.getByText("Prévue")).toBeInTheDocument();
      expect(screen.getAllByRole("button", { name: /Modifier la dépense/ })).toHaveLength(2);
    } finally {
      // @ts-expect-error nettoyage du mock jsdom
      delete window.matchMedia;
    }
  });
});

describe("VehicleExpenses", () => {
  it("affiche la liste, la puce Prévue et le total", async () => {
    renderExpenses();
    expect(await screen.findByText("Lavage complet")).toBeInTheDocument();
    expect(screen.getByText("Prévue")).toBeInTheDocument();
    expect(screen.getByTestId("expenses-total")).toHaveTextContent(/Réalisées : 85,50\s€ · Prévues : 100,00\s€/);
    expect(screen.getByText("Importée")).toBeInTheDocument();
    expect(screen.getByRole("table")).toBeInTheDocument();
  });

  it("n'affiche pas « Prévues » sans dépense prévue", async () => {
    api.listExpenses.mockResolvedValue([manual]);
    renderExpenses();
    await screen.findByText("Lavage complet");
    expect(screen.getByTestId("expenses-total")).not.toHaveTextContent("Prévues");
    expect(screen.queryByText("Importée")).toBeNull();
  });

  it("préremplit la date à aujourd'hui à l'ajout", async () => {
    renderExpenses();
    fireEvent.click(await screen.findByRole("button", { name: "Ajouter une dépense" }));
    expect(screen.getByLabelText(/^Date/)).toHaveValue(todayLocal());
  });

  it("masque modifier et supprimer pour une dépense non manuelle", async () => {
    renderExpenses();
    await screen.findByText("Carburant");
    expect(screen.getAllByRole("button", { name: /Modifier la dépense/ })).toHaveLength(2);
    expect(screen.queryByRole("button", { name: "Modifier la dépense du 02/05/2026" })).toBeNull();
  });

  it("affiche l'état vide", async () => {
    api.listExpenses.mockResolvedValue([]);
    renderExpenses();
    expect(await screen.findByText("Aucune dépense enregistrée pour ce véhicule.")).toBeInTheDocument();
  });

  it("masque toute écriture pour un véhicule sorti du parc", async () => {
    renderExpenses("Retired");
    await screen.findByText("Lavage complet");
    expect(screen.getByText("Véhicule sorti du parc : modification impossible.")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Ajouter une dépense" })).toBeNull();
    expect(screen.queryByRole("button", { name: /Modifier la dépense/ })).toBeNull();
    expect(screen.queryByRole("button", { name: /Supprimer la dépense/ })).toBeNull();
  });

  it("ajoute une dépense (montant zéro accepté, catégories actives seulement)", async () => {
    api.addExpense.mockResolvedValue(manual);
    renderExpenses();
    await fillValidForm();
    const submit = screen.getByRole("button", { name: "Ajouter" });
    expect(submit).toBeDisabled();
    fireEvent.change(screen.getByLabelText(/Montant/), { target: { value: "0" } });
    fireEvent.change(screen.getByLabelText(/Libellé/), { target: { value: " Parking gare " } });
    fireEvent.click(submit);
    await waitFor(() =>
      expect(api.addExpense).toHaveBeenCalledWith("v1", { date: "2026-06-01", categoryId: "c2", amount: 0, label: "Parking gare" }),
    );
  });

  it("refuse un montant négatif ou à plus de deux décimales avant envoi", async () => {
    renderExpenses();
    await fillValidForm();
    for (const bad of ["-5", "1,234"]) {
      fireEvent.change(screen.getByLabelText(/Montant/), { target: { value: bad } });
      expect(screen.getByRole("button", { name: "Ajouter" })).toBeDisabled();
    }
  });

  it("traduit l'erreur véhicule sorti du parc", async () => {
    api.addExpense.mockRejectedValue(problem("vehicle.retired"));
    renderExpenses();
    await fillValidForm();
    fireEvent.change(screen.getByLabelText(/Montant/), { target: { value: "5" } });
    fireEvent.click(screen.getByRole("button", { name: "Ajouter" }));
    expect(await screen.findByText(/sorti du parc/)).toBeInTheDocument();
  });

  it("modifie une dépense", async () => {
    api.updateExpense.mockResolvedValue(manual);
    renderExpenses();
    fireEvent.click(await screen.findByRole("button", { name: "Modifier la dépense du 10/05/2026" }));
    await waitFor(() => expect(screen.getByRole("combobox", { name: /Catégorie/ })).toHaveTextContent("Lavage"));
    fireEvent.change(screen.getByLabelText(/Montant/), { target: { value: "30,5" } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));
    await waitFor(() =>
      expect(api.updateExpense).toHaveBeenCalledWith("v1", "e1", { date: "2026-05-10", categoryId: "c1", amount: 30.5, label: "Lavage complet" }),
    );
  });

  it("supprime après confirmation", async () => {
    api.deleteExpense.mockResolvedValue(undefined);
    renderExpenses();
    fireEvent.click(await screen.findByRole("button", { name: "Supprimer la dépense du 10/05/2026" }));
    expect(api.deleteExpense).not.toHaveBeenCalled();
    fireEvent.click(screen.getByRole("button", { name: "Supprimer" }));
    await waitFor(() => expect(api.deleteExpense).toHaveBeenCalledWith("v1", "e1"));
  });
});
