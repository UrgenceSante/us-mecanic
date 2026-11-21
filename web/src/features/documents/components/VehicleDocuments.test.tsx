import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AxiosError, type AxiosResponse, type InternalAxiosRequestConfig } from "axios";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import VehicleDocuments from "./VehicleDocuments";

const api = vi.hoisted(() => ({ listDocuments: vi.fn(), addDocument: vi.fn(), deleteDocument: vi.fn(), fetchDocumentContent: vi.fn() }));
const cats = vi.hoisted(() => ({ listDocumentCategories: vi.fn() }));
vi.mock("../api/documentsApi", () => api);
vi.mock("../../document-categories/api/documentCategoriesApi", () => cats);
vi.mock("../../../hooks/useNotifSnack", () => ({
  default: () => ({ notifySuccess: vi.fn(), notifyError: vi.fn() }),
}));

const doc = {
  id: "d1", vehicleId: "v1", categoryId: "c1", interventionId: null, expenseId: null, label: "Carte grise 2026",
  fileName: "scan.pdf", contentType: "application/pdf", sizeBytes: 2048, addedAt: "2026-05-10T10:00:00Z",
};
const unnamed = { ...doc, id: "d2", label: null, fileName: "photo.jpg", categoryId: "c2", sizeBytes: 3 * 1024 * 1024 };

function problem(code: string) {
  const response = { status: 400, data: { code, title: "Erreur" } } as AxiosResponse;
  return new AxiosError("Erreur", "ERR_BAD_REQUEST", {} as InternalAxiosRequestConfig, null, response);
}

function renderSection() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={client}>
      <VehicleDocuments vehicleId="v1" />
    </QueryClientProvider>,
  );
}

function stubMobile(matches: boolean) {
  vi.stubGlobal("matchMedia", (query: string) => ({
    matches, media: query, addEventListener: vi.fn(), removeEventListener: vi.fn(), addListener: vi.fn(), removeListener: vi.fn(),
  }));
}

beforeEach(() => {
  vi.resetAllMocks();
  api.listDocuments.mockResolvedValue([doc, unnamed]);
  const all = [
    { id: "c1", name: "Carte grise", isActive: true },
    { id: "c2", name: "Photo", isActive: true },
    { id: "c3", name: "Ancienne", isActive: false },
  ];
  cats.listDocumentCategories.mockImplementation((includeInactive: boolean) =>
    Promise.resolve(includeInactive ? all : all.filter((c) => c.isActive)),
  );
});
afterEach(() => vi.unstubAllGlobals());

async function openAddDialog() {
  fireEvent.click(await screen.findByRole("button", { name: "Ajouter un document" }));
  return screen.findByTestId("document-file-input");
}
async function chooseCategory(name: string) {
  fireEvent.mouseDown(screen.getByRole("combobox", { name: /^Catégorie/ }));
  fireEvent.click(await screen.findByRole("option", { name }));
}
const pdf = (size = 100) => new File([new Uint8Array(size)], "contrat.pdf", { type: "application/pdf" });

describe("VehicleDocuments", () => {
  it("liste les documents avec libellé ou nom d'origine, catégorie, date et taille", async () => {
    renderSection();
    expect(await screen.findByText("Carte grise 2026")).toBeTruthy();
    expect(screen.getByText("photo.jpg")).toBeTruthy();
    expect(screen.getAllByText("10/05/2026")).toHaveLength(2);
    expect(screen.getByText(/2 Ko/)).toBeTruthy();
    expect(screen.getByText(/3 Mo/)).toBeTruthy();
    expect(screen.getByRole("heading", { level: 2, name: "Documents" })).toBeTruthy();
  });

  it("affiche des cartes sur mobile", async () => {
    stubMobile(true);
    renderSection();
    await screen.findByText("Carte grise 2026");
    expect(screen.getAllByTestId("list-card")).toHaveLength(2);
  });

  it("affiche « Aucun document » avec le bouton d'ajout", async () => {
    api.listDocuments.mockResolvedValue([]);
    renderSection();
    expect(await screen.findByText("Aucun document")).toBeTruthy();
    expect(screen.getByRole("button", { name: "Ajouter un document" })).toBeTruthy();
  });

  it("filtre par catégorie", async () => {
    renderSection();
    await screen.findByText("Carte grise 2026");
    fireEvent.mouseDown(screen.getByRole("combobox", { name: "Catégorie" }));
    fireEvent.click(await screen.findByRole("option", { name: "Photo" }));
    await waitFor(() => expect(api.listDocuments).toHaveBeenLastCalledWith("v1", "c2"));
  });

  it("ajoute un fichier avec catégorie et libellé", async () => {
    api.addDocument.mockResolvedValue(doc);
    renderSection();
    const input = await openAddDialog();
    fireEvent.change(input, { target: { files: [pdf()] } });
    await chooseCategory("Photo");
    expect(screen.queryByRole("option", { name: "Ancienne" })).toBeNull();
    fireEvent.change(screen.getByLabelText(/Libellé/), { target: { value: "Contrat" } });
    fireEvent.click(screen.getByRole("button", { name: "Ajouter" }));
    await waitFor(() => expect(api.addDocument).toHaveBeenCalled());
    expect(api.addDocument.mock.calls[0][0]).toBe("v1");
    expect(api.addDocument.mock.calls[0][1]).toMatchObject({ categoryId: "c2", label: "Contrat" });
    await waitFor(() => expect(screen.queryByRole("dialog")).toBeNull());
  });

  it("exige fichier et catégorie", async () => {
    renderSection();
    await openAddDialog();
    fireEvent.click(screen.getByRole("button", { name: "Ajouter" }));
    expect(await screen.findByText("Le fichier est obligatoire.")).toBeTruthy();
    expect(screen.getByText("La catégorie est obligatoire.")).toBeTruthy();
    expect(api.addDocument).not.toHaveBeenCalled();
  });

  it("refuse un mauvais format avant envoi", async () => {
    renderSection();
    const input = await openAddDialog();
    fireEvent.change(input, { target: { files: [new File(["x"], "a.heic", { type: "image/heic" })] } });
    expect(await screen.findByText("Format non accepté : PDF, JPEG ou PNG uniquement.")).toBeTruthy();
  });

  it("refuse un fichier de plus de 10 Mo avant envoi", async () => {
    renderSection();
    const input = await openAddDialog();
    fireEvent.change(input, { target: { files: [pdf(10 * 1024 * 1024 + 1)] } });
    expect(await screen.findByText("Fichier trop volumineux (10 Mo maximum).")).toBeTruthy();
  });

  it("conserve la saisie et affiche l'erreur de l'API", async () => {
    api.addDocument.mockRejectedValue(problem("document.file_type_unsupported"));
    renderSection();
    const input = await openAddDialog();
    fireEvent.change(input, { target: { files: [pdf()] } });
    await chooseCategory("Photo");
    fireEvent.click(screen.getByRole("button", { name: "Ajouter" }));
    expect(await screen.findByText("Format non accepté : PDF, JPEG ou PNG uniquement.")).toBeTruthy();
    expect(screen.getByText(/contrat\.pdf/)).toBeTruthy();
  });

  it("propose la prise de photo sur mobile seulement", async () => {
    stubMobile(true);
    renderSection();
    await openAddDialog();
    expect(screen.getByRole("button", { name: "Prendre une photo" })).toBeTruthy();
    expect(screen.getByTestId("document-camera-input").getAttribute("capture")).toBe("environment");
  });

  it("supprime après confirmation qui nomme le document", async () => {
    api.deleteDocument.mockResolvedValue(undefined);
    renderSection();
    fireEvent.click(await screen.findByRole("button", { name: "Supprimer Carte grise 2026" }));
    expect(screen.getByText(/Supprimer « Carte grise 2026 » \? Cette action est définitive\./)).toBeTruthy();
    fireEvent.click(screen.getByRole("button", { name: "Supprimer" }));
    await waitFor(() => expect(api.deleteDocument).toHaveBeenCalledWith("v1", "d1"));
  });

  it("ouvre dans un onglet ouvert au clic, avec une URL locale", async () => {
    const tab = { location: { href: "" }, close: vi.fn() };
    const open = vi.fn().mockReturnValue(tab);
    vi.stubGlobal("open", open);
    URL.createObjectURL = vi.fn().mockReturnValue("blob:doc");
    URL.revokeObjectURL = vi.fn();
    api.fetchDocumentContent.mockResolvedValue(new Blob(["x"]));
    renderSection();
    fireEvent.click(await screen.findByRole("button", { name: "Ouvrir Carte grise 2026" }));
    expect(open).toHaveBeenCalledTimes(1);
    await waitFor(() => expect(tab.location.href).toBe("blob:doc"));
    expect(api.fetchDocumentContent).toHaveBeenCalledWith("v1", "d1");
  });

  it("télécharge sous le nom d'origine puis révoque l'URL", async () => {
    URL.createObjectURL = vi.fn().mockReturnValue("blob:dl");
    URL.revokeObjectURL = vi.fn();
    const click = vi.spyOn(HTMLAnchorElement.prototype, "click").mockImplementation(function (this: HTMLAnchorElement) {
      expect(this.download).toBe("scan.pdf");
    });
    api.fetchDocumentContent.mockResolvedValue(new Blob(["x"]));
    renderSection();
    fireEvent.click(await screen.findByRole("button", { name: "Télécharger Carte grise 2026" }));
    await waitFor(() => expect(URL.revokeObjectURL).toHaveBeenCalledWith("blob:dl"));
    expect(click).toHaveBeenCalled();
    click.mockRestore();
  });
});
