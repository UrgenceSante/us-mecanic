import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { fireEvent, render, screen, waitFor, within } from "@testing-library/react";
import { AxiosError, type AxiosResponse, type InternalAxiosRequestConfig } from "axios";
import { MemoryRouter, Route, Routes } from "react-router";
import { beforeEach, describe, expect, it, vi } from "vitest";
import ProblemDetailPage from "./ProblemDetailPage";

const api = vi.hoisted(() => ({
  getProblem: vi.fn(),
  analyzeProblem: vi.fn(),
  addProblemAction: vi.fn(),
  updateProblemAction: vi.fn(),
  closeProblemAction: vi.fn(),
}));
const referencesApi = vi.hoisted(() => ({ listProblemReferences: vi.fn() }));
const interventionsApi = vi.hoisted(() => ({ listInterventions: vi.fn() }));
const vehiclesApi = vi.hoisted(() => ({ getVehicle: vi.fn() }));
vi.mock("../api/problemsApi", () => api);
vi.mock("../../problem-references/api/problemReferencesApi", () => referencesApi);
vi.mock("../../maintenance/api/interventionsApi", () => interventionsApi);
vi.mock("../../vehicles/api/vehiclesApi", () => vehiclesApi);
vi.mock("../../../hooks/useNotifSnack", () => ({
  default: () => ({ notifySuccess: vi.fn(), notifyError: vi.fn() }),
}));

const references: Record<string, { id: string; name: string; isActive: boolean }[]> = {
  "api/problem-natures": [
    { id: "n1", name: "Mécanique", isActive: true },
    { id: "n0", name: "Ancienne nature", isActive: false },
  ],
  "api/problem-elements": [{ id: "e1", name: "Freins", isActive: true }],
  "api/problem-actors": [
    { id: "a1", name: "Garage", isActive: true },
    { id: "a0", name: "Ancien acteur", isActive: false },
  ],
  "api/problem-action-types": [{ id: "t1", name: "Réparer", isActive: true }],
  "api/problem-constraints": [{ id: "c1", name: "Bloquante", isActive: true }],
};

const reported = {
  id: "p1",
  vehicleId: "v1",
  date: "2026-03-04",
  description: "Freins HS",
  author: "Jean",
  status: "Reported",
  recordedAt: "2026-03-04T08:00:00Z",
  analysis: null,
  actions: [],
};
const analysis = { natureId: "n0", elementId: "e1", text: "Plaquettes usées", immobilized: true, analyzedAt: "2026-03-05T08:00:00Z" };
const action = {
  id: "act1",
  problemId: "p1",
  actorId: "a0",
  typeId: "t1",
  constraintId: "c1",
  dueDate: "2026-04-01",
  comment: "Prendre RDV",
  interventionId: null,
  isClosed: false,
  closedAt: null,
  recordedAt: "2026-03-06T08:00:00Z",
};
const analyzed = { ...reported, status: "Analyzed", analysis, actions: [action] };

function apiError(status: number, code: string, title: string) {
  const response = { status, data: { code, title } } as AxiosResponse;
  return new AxiosError(title, "ERR", {} as InternalAxiosRequestConfig, null, response);
}

function renderPage() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={client}>
      <MemoryRouter initialEntries={["/problems/p1"]}>
        <Routes>
          <Route path="/problems/:problemId" element={<ProblemDetailPage />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

const choose = async (label: RegExp, option: string) => {
  fireEvent.mouseDown(screen.getByRole("combobox", { name: label }));
  fireEvent.click(within(await screen.findByRole("listbox")).getByText(option));
};

function setupMocks() {
  vi.resetAllMocks();
  api.getProblem.mockResolvedValue(reported);
  referencesApi.listProblemReferences.mockImplementation((route: string, includeInactive: boolean) =>
    Promise.resolve(references[route].filter((r) => includeInactive || r.isActive)),
  );
  interventionsApi.listInterventions.mockResolvedValue([
    { id: "i1", vehicleId: "v1", date: "2026-02-01", work: "Vidange", deadlines: [] },
  ]);
  vehiclesApi.getVehicle.mockResolvedValue({ id: "v1", licensePlate: "AB-123-CD" });
}

describe("ProblemDetailPage - analyse", () => {
  beforeEach(setupMocks);

  it("affiche le problème avec le lien véhicule", async () => {
    renderPage();
    expect(await screen.findByText("Freins HS")).toBeInTheDocument();
    expect(screen.getByText("04/03/2026")).toBeInTheDocument();
    expect(screen.getByText("Jean")).toBeInTheDocument();
    expect(screen.getByText("Signalé")).toBeInTheDocument();
    expect(await screen.findByRole("link", { name: "AB-123-CD" })).toHaveAttribute("href", "/vehicles/v1");
    expect(screen.queryByRole("button", { name: "Ajouter une action" })).not.toBeInTheDocument();
  });

  it("envoie l'analyse avec des listes limitées aux valeurs actives puis affiche le problème analysé", async () => {
    api.analyzeProblem.mockResolvedValue(analyzed);
    renderPage();
    await screen.findByText("Freins HS");

    fireEvent.mouseDown(screen.getByRole("combobox", { name: /Nature/ }));
    const natures = within(await screen.findByRole("listbox"));
    expect(natures.getByText("Mécanique")).toBeInTheDocument();
    expect(natures.queryByText("Ancienne nature")).not.toBeInTheDocument();
    fireEvent.click(natures.getByText("Mécanique"));
    await choose(/Élément/, "Freins");
    fireEvent.change(screen.getByLabelText(/Analyse/, { selector: "textarea" }), { target: { value: " Plaquettes usées " } });
    fireEvent.click(screen.getByRole("checkbox", { name: "Véhicule immobilisé" }));

    api.getProblem.mockResolvedValue(analyzed);
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer l'analyse" }));

    await waitFor(() =>
      expect(api.analyzeProblem).toHaveBeenCalledWith("p1", {
        natureId: "n1",
        elementId: "e1",
        analysis: "Plaquettes usées",
        immobilized: true,
      }),
    );
    expect(await screen.findByText("Analysé")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Enregistrer l'analyse" })).not.toBeInTheDocument();
    expect(screen.getByText("Ancienne nature (désactivé)")).toBeInTheDocument();
  });

  it("traduit l'erreur d'une analyse déjà faite", async () => {
    api.analyzeProblem.mockRejectedValue(apiError(409, "problem.already_analyzed", "Déjà analysé"));
    renderPage();
    await screen.findByText("Freins HS");
    await choose(/Nature/, "Mécanique");
    await choose(/Élément/, "Freins");
    fireEvent.change(screen.getByLabelText(/Analyse/, { selector: "textarea" }), { target: { value: "Texte" } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer l'analyse" }));
    expect(await screen.findByText("Ce problème a déjà été analysé : l'analyse n'est pas modifiable.")).toBeInTheDocument();
  });

});

describe("ProblemDetailPage - actions", () => {
  beforeEach(setupMocks);

  it("affiche l'analyse en lecture et les actions, y compris les valeurs désactivées", async () => {
    api.getProblem.mockResolvedValue(analyzed);
    renderPage();
    expect(await screen.findByText("Plaquettes usées")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Enregistrer l'analyse" })).not.toBeInTheDocument();
    const row = within(await screen.findByRole("row", { name: /Prendre RDV/ }));
    expect(await row.findByText("Ancien acteur (désactivé)")).toBeInTheDocument();
    expect(row.getByText("Réparer")).toBeInTheDocument();
    expect(row.getByText("Bloquante")).toBeInTheDocument();
    expect(row.getByText("01/04/2026")).toBeInTheDocument();
    expect(row.getByText("Ouverte")).toBeInTheDocument();
  });

  it("ajoute une action avec une intervention du véhicule", async () => {
    api.getProblem.mockResolvedValue({ ...analyzed, actions: [] });
    api.addProblemAction.mockResolvedValue(action);
    renderPage();
    fireEvent.click(await screen.findByRole("button", { name: "Ajouter une action" }));

    fireEvent.mouseDown(await screen.findByRole("combobox", { name: /Acteur/ }));
    const actors = within(await screen.findByRole("listbox"));
    expect(actors.queryByText("Ancien acteur")).not.toBeInTheDocument();
    fireEvent.click(actors.getByText("Garage"));
    await choose(/Type d'action/, "Réparer");
    await choose(/Contrainte/, "Bloquante");
    fireEvent.change(screen.getByLabelText("Échéance"), { target: { value: "2026-05-01" } });
    fireEvent.change(screen.getByLabelText("Commentaire"), { target: { value: "Urgent" } });
    await choose(/Intervention liée/, "01/02/2026 - Vidange");
    expect(interventionsApi.listInterventions).toHaveBeenCalledWith("v1");

    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));
    await waitFor(() =>
      expect(api.addProblemAction).toHaveBeenCalledWith("p1", {
        actorId: "a1",
        typeId: "t1",
        constraintId: "c1",
        dueDate: "2026-05-01",
        comment: "Urgent",
        interventionId: "i1",
      }),
    );
  });

  it("traduit l'erreur d'un référentiel désactivé à l'ajout", async () => {
    api.getProblem.mockResolvedValue(analyzed);
    api.addProblemAction.mockRejectedValue(apiError(409, "problem_actor.inactive", "Désactivé"));
    renderPage();
    fireEvent.click(await screen.findByRole("button", { name: "Ajouter une action" }));
    await choose(/Acteur/, "Garage");
    await choose(/Type d'action/, "Réparer");
    await choose(/Contrainte/, "Bloquante");
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));
    expect(await screen.findByText("Cet acteur est désactivé : choisissez-en un autre.")).toBeInTheDocument();
  });

  it("modifie échéance, commentaire et intervention en renvoyant les trois champs", async () => {
    api.getProblem.mockResolvedValue(analyzed);
    api.updateProblemAction.mockResolvedValue({ ...action, comment: "RDV pris" });
    renderPage();
    fireEvent.click(await screen.findByRole("button", { name: "Modifier" }));
    fireEvent.change(screen.getByLabelText("Commentaire"), { target: { value: "RDV pris" } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));
    await waitFor(() =>
      expect(api.updateProblemAction).toHaveBeenCalledWith("p1", "act1", {
        dueDate: "2026-04-01",
        comment: "RDV pris",
        interventionId: null,
      }),
    );
  });

  it("clôture une action après confirmation et affiche le statut résolu", async () => {
    api.getProblem.mockResolvedValue(analyzed);
    api.closeProblemAction.mockResolvedValue({ ...action, isClosed: true, closedAt: "2026-07-10T22:30:00Z" });
    renderPage();
    fireEvent.click(await screen.findByRole("button", { name: "Clôturer" }));
    expect(api.closeProblemAction).not.toHaveBeenCalled();

    api.getProblem.mockResolvedValue({
      ...analyzed,
      status: "Resolved",
      actions: [{ ...action, isClosed: true, closedAt: "2026-07-10T22:30:00Z" }],
    });
    const dialog = within(await screen.findByRole("dialog"));
    fireEvent.click(dialog.getByRole("button", { name: "Clôturer" }));
    await waitFor(() => expect(api.closeProblemAction).toHaveBeenCalledWith("p1", "act1"));
    expect(await screen.findByText("Résolu")).toBeInTheDocument();
    expect(await screen.findByText("Clôturée le 11/07/2026")).toBeInTheDocument();
  });

  it("traduit l'erreur d'une action déjà clôturée", async () => {
    api.getProblem.mockResolvedValue(analyzed);
    api.closeProblemAction.mockRejectedValue(apiError(409, "problem_action.already_closed", "Close"));
    renderPage();
    fireEvent.click(await screen.findByRole("button", { name: "Clôturer" }));
    const dialog = within(await screen.findByRole("dialog"));
    fireEvent.click(dialog.getByRole("button", { name: "Clôturer" }));
    expect(await screen.findByText("Cette action est déjà clôturée.")).toBeInTheDocument();
  });

  it("repasse à Analysé à l'écran quand on ajoute une action sur un problème Résolu", async () => {
    const closed = { ...action, isClosed: true, closedAt: "2026-03-10T08:00:00Z" };
    api.getProblem.mockResolvedValue({ ...analyzed, status: "Resolved", actions: [closed] });
    api.addProblemAction.mockResolvedValue({ ...action, id: "act2" });
    renderPage();
    expect(await screen.findByText("Résolu")).toBeInTheDocument();
    fireEvent.click(screen.getByRole("button", { name: "Ajouter une action" }));
    await choose(/Acteur/, "Garage");
    await choose(/Type d'action/, "Réparer");
    await choose(/Contrainte/, "Bloquante");
    api.getProblem.mockResolvedValue({ ...analyzed, actions: [closed, { ...action, id: "act2" }] });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));
    expect(await screen.findByText("Analysé")).toBeInTheDocument();
    expect(screen.queryByText("Résolu")).not.toBeInTheDocument();
  });

  it("affiche un message si le problème est introuvable", async () => {
    api.getProblem.mockRejectedValue(apiError(404, "problem.not_found", "Introuvable"));
    renderPage();
    expect(await screen.findByText("Problème introuvable.")).toBeInTheDocument();
  });
});
