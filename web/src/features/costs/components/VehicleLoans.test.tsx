import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AxiosError, type AxiosResponse, type InternalAxiosRequestConfig } from "axios";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { loanEndMonth } from "../types/loan";
import VehicleLoans from "./VehicleLoans";

const api = vi.hoisted(() => ({ listLoans: vi.fn(), addLoan: vi.fn(), updateLoan: vi.fn(), deleteLoan: vi.fn() }));
vi.mock("../api/loansApi", () => api);
vi.mock("../../../hooks/useNotifSnack", () => ({
  default: () => ({ notifySuccess: vi.fn(), notifyError: vi.fn() }),
}));

function problem(code: string) {
  const response = { status: 409, data: { code, title: "Erreur" } } as AxiosResponse;
  return new AxiosError("Erreur", "ERR_BAD_REQUEST", {} as InternalAxiosRequestConfig, null, response);
}

const loan = {
  id: "l1", vehicleId: "v1", lender: "Crédit Test", borrowedAmount: 20000, monthlyPayment: 350.5,
  startDate: "2026-11-15", durationMonths: 3, recordedAt: "2026-05-01T10:00:00Z",
};

function renderLoans(status: "Active" | "Retired" = "Active") {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={client}>
      <VehicleLoans vehicleId="v1" status={status} />
    </QueryClientProvider>,
  );
}

describe("loanEndMonth", () => {
  it("compte une mensualité par mois à partir du mois de début", () => {
    expect(loanEndMonth("2026-11-15", 3)).toBe("01/2027");
    expect(loanEndMonth("2026-01-01", 1)).toBe("01/2026");
    expect(loanEndMonth("2026-12-31", 12)).toBe("11/2027");
  });
});

describe("rendu mobile", () => {
  it("affiche des cartes empilées sous le breakpoint sm", async () => {
    vi.resetAllMocks();
    api.listLoans.mockResolvedValue([loan]);
    window.matchMedia = vi.fn().mockImplementation((query: string) => ({
      matches: true, media: query, addEventListener: vi.fn(), removeEventListener: vi.fn(),
      addListener: vi.fn(), removeListener: vi.fn(), dispatchEvent: vi.fn(), onchange: null,
    }));
    try {
      renderLoans();
      expect(await screen.findByTestId("loan-card")).toBeInTheDocument();
      expect(screen.queryByRole("table")).toBeNull();
      expect(screen.getByText(/350,50\s€\/mois/)).toBeInTheDocument();
      expect(screen.getByText(/15\/11\/2026 → 01\/2027 \(3 mois\)/)).toBeInTheDocument();
      expect(screen.getByRole("button", { name: /Modifier l'emprunt/ })).toBeInTheDocument();
    } finally {
      // @ts-expect-error nettoyage du mock jsdom
      delete window.matchMedia;
    }
  });
});

describe("VehicleLoans", () => {
  beforeEach(() => {
    vi.resetAllMocks();
    api.listLoans.mockResolvedValue([loan]);
  });

  it("affiche la liste avec montants et fin calculée", async () => {
    renderLoans();
    expect(await screen.findByText("Crédit Test")).toBeInTheDocument();
    expect(screen.getByText(/20\s000,00\s€/)).toBeInTheDocument();
    expect(screen.getByText(/350,50\s€/)).toBeInTheDocument();
    expect(screen.getByText("15/11/2026")).toBeInTheDocument();
    expect(screen.getByText("3 mois")).toBeInTheDocument();
    expect(screen.getByText("01/2027")).toBeInTheDocument();
    expect(screen.getByRole("table")).toBeInTheDocument();
  });

  it("affiche l'état vide", async () => {
    api.listLoans.mockResolvedValue([]);
    renderLoans();
    expect(await screen.findByText("Aucun emprunt enregistré pour ce véhicule.")).toBeInTheDocument();
  });

  it("masque l'ajout pour un véhicule sorti du parc mais garde modifier et supprimer", async () => {
    renderLoans("Retired");
    await screen.findByText("Crédit Test");
    expect(screen.queryByRole("button", { name: "Ajouter un emprunt" })).toBeNull();
    expect(screen.getByRole("button", { name: /Modifier l'emprunt/ })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /Supprimer l'emprunt/ })).toBeInTheDocument();
  });

  it("ajoute un emprunt avec montants parsés", async () => {
    api.addLoan.mockResolvedValue(loan);
    renderLoans();
    fireEvent.click(await screen.findByRole("button", { name: "Ajouter un emprunt" }));
    const submit = screen.getByRole("button", { name: "Ajouter" });
    expect(submit).toBeDisabled();
    fireEvent.change(screen.getByLabelText(/Organisme prêteur/), { target: { value: " Banque " } });
    fireEvent.change(screen.getByLabelText(/Montant emprunté/), { target: { value: "12 000,5" } });
    fireEvent.change(screen.getByLabelText(/Mensualité/), { target: { value: "300" } });
    fireEvent.change(screen.getByLabelText(/Date de début/), { target: { value: "2026-12-01" } });
    fireEvent.change(screen.getByLabelText(/Durée/), { target: { value: "24" } });
    fireEvent.click(submit);
    await waitFor(() =>
      expect(api.addLoan).toHaveBeenCalledWith("v1", {
        lender: "Banque", borrowedAmount: 12000.5, monthlyPayment: 300, startDate: "2026-12-01", durationMonths: 24,
      }),
    );
  });

  it("traduit l'erreur véhicule sorti du parc", async () => {
    api.addLoan.mockRejectedValue(problem("vehicle.retired"));
    renderLoans();
    fireEvent.click(await screen.findByRole("button", { name: "Ajouter un emprunt" }));
    fireEvent.change(screen.getByLabelText(/Organisme prêteur/), { target: { value: "B" } });
    fireEvent.change(screen.getByLabelText(/Montant emprunté/), { target: { value: "1" } });
    fireEvent.change(screen.getByLabelText(/Mensualité/), { target: { value: "1" } });
    fireEvent.change(screen.getByLabelText(/Date de début/), { target: { value: "2026-12-01" } });
    fireEvent.change(screen.getByLabelText(/Durée/), { target: { value: "2" } });
    fireEvent.click(screen.getByRole("button", { name: "Ajouter" }));
    expect(await screen.findByText(/sorti du parc/)).toBeInTheDocument();
  });

  it("modifie la durée d'un emprunt", async () => {
    api.updateLoan.mockResolvedValue(loan);
    renderLoans();
    fireEvent.click(await screen.findByRole("button", { name: /Modifier l'emprunt/ }));
    fireEvent.change(screen.getByLabelText(/Durée/), { target: { value: "2" } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));
    await waitFor(() =>
      expect(api.updateLoan).toHaveBeenCalledWith("v1", "l1", {
        lender: "Crédit Test", borrowedAmount: 20000, monthlyPayment: 350.5, startDate: "2026-11-15", durationMonths: 2,
      }),
    );
  });

  it("supprime après confirmation", async () => {
    api.deleteLoan.mockResolvedValue(undefined);
    renderLoans();
    fireEvent.click(await screen.findByRole("button", { name: /Supprimer l'emprunt/ }));
    expect(api.deleteLoan).not.toHaveBeenCalled();
    fireEvent.click(screen.getByRole("button", { name: "Supprimer" }));
    await waitFor(() => expect(api.deleteLoan).toHaveBeenCalledWith("v1", "l1"));
  });
});
