import { afterEach, describe, expect, it, vi } from "vitest";
import {
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router";
import AppLayout from "./AppLayout";

vi.mock("../components/User/UserAvatar", () => ({ default: () => null }));

function mockMatchMedia(matches: boolean) {
  window.matchMedia = vi.fn().mockImplementation((query: string) => ({
    matches,
    media: query,
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    addListener: vi.fn(),
    removeListener: vi.fn(),
    dispatchEvent: vi.fn(),
    onchange: null,
  }));
}

function renderLayout(path = "/") {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <Routes>
        <Route path="/" element={<AppLayout />}>
          <Route index element={<div>Accueil</div>} />
          <Route path="vehicles" element={<div>Page parc</div>} />
          <Route path="garages" element={<div>Page garages</div>} />
        </Route>
      </Routes>
    </MemoryRouter>,
  );
}

afterEach(() => vi.restoreAllMocks());

describe("AppLayout desktop", () => {
  it("affiche les entrées du quotidien et le menu Administration", async () => {
    mockMatchMedia(true);
    renderLayout("/vehicles");
    expect(screen.getByRole("link", { name: "Tableau de bord" })).toBeTruthy();
    expect(
      screen.getByRole("link", { name: "Parc" }).getAttribute("aria-current"),
    ).toBe("page");
    expect(screen.queryByRole("link", { name: "Échéances" })).toBeNull();
    expect(screen.queryByRole("link", { name: "Garages" })).toBeNull();

    fireEvent.click(screen.getByRole("button", { name: /Administration/ }));
    for (const label of [
      "Types de véhicule",
      "Types d'échéance",
      "Garages",
      "Catégories de dépense",
      "Référentiels problèmes",
    ]) {
      expect(screen.getByRole("menuitem", { name: label })).toBeTruthy();
    }
    fireEvent.click(screen.getByRole("menuitem", { name: "Garages" }));
    expect(await screen.findByText("Page garages")).toBeTruthy();
  });
});

describe("AppLayout mobile", () => {
  it("ouvre le tiroir, groupe les entrées et le ferme après navigation", async () => {
    mockMatchMedia(false);
    renderLayout("/vehicles");
    expect(screen.queryByRole("button", { name: /Administration/ })).toBeNull();

    fireEvent.click(screen.getByRole("button", { name: "Ouvrir le menu" }));
    const suivi = screen.getByRole("list", { name: "Suivi" });
    const admin = screen.getByRole("list", { name: "Administration" });
    expect(
      within(suivi)
        .getByRole("link", { name: "Parc" })
        .getAttribute("aria-current"),
    ).toBe("page");

    fireEvent.click(within(admin).getByRole("link", { name: "Garages" }));
    expect(await screen.findByText("Page garages")).toBeTruthy();
    await waitFor(() =>
      expect(screen.queryByRole("list", { name: "Suivi" })).toBeNull(),
    );
  });
});
