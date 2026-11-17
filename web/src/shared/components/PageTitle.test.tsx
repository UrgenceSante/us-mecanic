import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import PageTitle from "./PageTitle";

describe("PageTitle", () => {
  it("rend un h1 et fixe document.title au format « page — Urgence Santé »", () => {
    render(<PageTitle>Parc de véhicules</PageTitle>);
    expect(screen.getByRole("heading", { level: 1, name: "Parc de véhicules" })).toBeTruthy();
    expect(document.title).toBe("Parc de véhicules — Urgence Santé");
  });

  it("restaure le titre précédent au démontage", () => {
    document.title = "Avant";
    const { unmount } = render(<PageTitle>Garages</PageTitle>);
    unmount();
    expect(document.title).toBe("Avant");
  });
});
